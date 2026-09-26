using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// Discovers operations declared by MVC controllers.
/// </summary>
internal static class ControllerDiscovery
{
	private static readonly Dictionary<string, string> _methodAttributes = new()
	{
		["HttpGetAttribute"] = "get",
		["HttpPostAttribute"] = "post",
		["HttpPutAttribute"] = "put",
		["HttpDeleteAttribute"] = "delete",
		["HttpPatchAttribute"] = "patch",
		["HttpHeadAttribute"] = "head",
		["HttpOptionsAttribute"] = "options"
	};

	/// <summary>
	/// Discovers the operations declared by a controller type.
	/// </summary>
	/// <param name="type">The controller type.</param>
	/// <returns>The operations.</returns>
	public static IEnumerable<EndpointInfo> Discover(INamedTypeSymbol type)
	{
		var prefix = GetRoutePrefix(type);
		var documentNames = GetDocumentNames(type);

		foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
		{
			if (method.MethodKind != MethodKind.Ordinary) continue;
			if (method.DeclaredAccessibility != Accessibility.Public) continue;

			foreach (var endpoint in DiscoverActions(type, method, prefix, documentNames))
			{
				yield return endpoint;
			}
		}
	}

	/// <summary>
	/// Reads the descriptions a controller is placed in by `[OpenApiDocument]`.
	/// </summary>
	/// <remarks>
	/// An empty result means the default description, which is where a controller without
	/// the attribute belongs.
	/// </remarks>
	private static IReadOnlyList<string> GetDocumentNames(INamedTypeSymbol type)
	{
		foreach (var attribute in type.GetAttributes())
		{
			if (attribute.AttributeClass?.Name != "OpenApiDocumentAttribute") continue;
			if (attribute.ConstructorArguments.Length == 0) continue;

			// The single `params string[]` parameter arrives as one array argument.
			return attribute.ConstructorArguments[0].Values
				.Select(x => x.Value as string)
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => x!)
				.ToArray();
		}

		return [];
	}

	private static IEnumerable<EndpointInfo> DiscoverActions(
		INamedTypeSymbol type,
		IMethodSymbol method,
		string prefix,
		IReadOnlyList<string> documentNames)
	{
		foreach (var attribute in method.GetAttributes())
		{
			var attributeName = attribute.AttributeClass?.Name;
			if (attributeName is null || !_methodAttributes.TryGetValue(attributeName, out var httpMethod)) continue;

			var template = attribute.ConstructorArguments.Length > 0
				? attribute.ConstructorArguments[0].Value as string
				: null;

			// Qualified by the controller, since OpenAPI requires operation IDs to be unique
			// across the description and action names repeat across controllers.
			var endpoint = new EndpointInfo
			{
				Route = CombineRoute(prefix, template, type, method),
				Method = httpMethod,
				OperationId = $"{StripControllerSuffix(type.Name)}_{method.Name}"
			};

			endpoint.DocumentNames.AddRange(documentNames);

			AddParameters(endpoint, method);
			AddResponses(endpoint, method);

			EndpointMetadata.ApplyAttributes(endpoint, type);
			EndpointMetadata.ApplyAttributes(endpoint, method);
			EndpointMetadata.ApplyDocComment(endpoint, DocComment.Read(method));

			yield return endpoint;
		}
	}

	private static string GetRoutePrefix(INamedTypeSymbol type)
	{
		foreach (var attribute in type.GetAttributes())
		{
			if (attribute.AttributeClass?.Name != "RouteAttribute") continue;
			if (attribute.ConstructorArguments.Length == 0) continue;

			return attribute.ConstructorArguments[0].Value as string ?? string.Empty;
		}

		return string.Empty;
	}

	private static string CombineRoute(string prefix, string? template, INamedTypeSymbol type, IMethodSymbol method)
	{
		var combined = string.IsNullOrEmpty(template)
			? prefix
			: string.IsNullOrEmpty(prefix) ? template! : $"{prefix}/{template}";

		combined = combined
			.Replace("[controller]", StripControllerSuffix(type.Name))
			.Replace("[action]", method.Name);

		if (!combined.StartsWith("/"))
			combined = "/" + combined;

		return combined;
	}

	private static string StripControllerSuffix(string name) =>
		name.EndsWith("Controller") ? name.Substring(0, name.Length - "Controller".Length) : name;

	private static void AddParameters(EndpointInfo endpoint, IMethodSymbol method)
	{
		foreach (var parameter in method.Parameters)
		{
			var binding = GetBindingSource(parameter, endpoint.Route);

			if (binding == "body")
			{
				endpoint.RequestBodyTypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				endpoint.RequestBodyParameterName = parameter.Name;
				endpoint.RequestBodyIsValidated = HasGeneratedSchema(parameter.Type);
				continue;
			}

			if (binding == "services") continue;

			endpoint.Parameters.Add(new EndpointParameterInfo
			{
				Name = parameter.Name,
				Location = binding,
				Required = binding == "path" || !parameter.IsOptional,
				TypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			});
		}
	}

	private static string GetBindingSource(IParameterSymbol parameter, string route)
	{
		foreach (var attribute in parameter.GetAttributes())
		{
			switch (attribute.AttributeClass?.Name)
			{
				case "FromBodyAttribute": return "body";
				case "FromRouteAttribute": return "path";
				case "FromQueryAttribute": return "query";
				case "FromHeaderAttribute": return "header";
				case "FromServicesAttribute": return "services";
			}
		}

		// `[ApiController]` infers a body binding for complex types, and binds the rest from
		// the route where a segment names them, falling back to the query string.
		if (EndpointDiscoveryHelpers.LooksLikeBody(parameter.Type)) return "body";

		return EndpointDiscoveryHelpers.RouteBinds(route, parameter.Name) ? "path" : "query";
	}

	private static void AddResponses(EndpointInfo endpoint, IMethodSymbol method)
	{
		var declared = EndpointDiscoveryHelpers.GetDeclaredResponses(method);
		if (declared.Count > 0)
		{
			endpoint.Responses.AddRange(declared);
			return;
		}

		endpoint.Responses.Add(new EndpointResponseInfo
		{
			StatusCode = 200,
			TypeName = EndpointDiscoveryHelpers.GetReturnPayloadType(method),
			Description = "Success"
		});
	}

	private static bool HasGeneratedSchema(ITypeSymbol type) =>
		type.GetAttributes().Any(x => x.AttributeClass?.Name == "GenerateJsonSchemaAttribute");
}
