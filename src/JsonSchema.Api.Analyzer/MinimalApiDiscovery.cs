using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// Discovers operations declared by minimal-API route registrations.
/// </summary>
internal static class MinimalApiDiscovery
{
	private static readonly Dictionary<string, string> _mapMethods = new()
	{
		["MapGet"] = "get",
		["MapPost"] = "post",
		["MapPut"] = "put",
		["MapDelete"] = "delete",
		["MapPatch"] = "patch"
	};

	/// <summary>
	/// Indicates whether a node might be a route registration, cheaply and syntactically.
	/// </summary>
	public static bool CouldBeRegistration(SyntaxNode node) =>
		node is InvocationExpressionSyntax
		{
			Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: var name }
		} && _mapMethods.ContainsKey(name);

	/// <summary>
	/// Discovers the operation declared by a route registration.
	/// </summary>
	/// <param name="invocation">The `Map*` invocation.</param>
	/// <param name="semanticModel">The semantic model for the invocation's tree.</param>
	/// <returns>The operation, or null if this is not a route registration.</returns>
	public static EndpointInfo? Discover(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
	{
		if (invocation.Expression is not MemberAccessExpressionSyntax member) return null;
		if (!_mapMethods.TryGetValue(member.Name.Identifier.ValueText, out var httpMethod)) return null;

		if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol) return null;
		if (symbol.ContainingType?.ToDisplayString() != "Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions") return null;

		var arguments = invocation.ArgumentList.Arguments;
		if (arguments.Count < 2) return null;

		if (semanticModel.GetConstantValue(arguments[0].Expression).Value is not string template) return null;

		var prefix = ResolveGroupPrefix(member.Expression, semanticModel);

		var endpoint = new EndpointInfo
		{
			Route = Combine(prefix, template),
			Method = httpMethod
		};

		AddHandler(endpoint, arguments[1].Expression, semanticModel);
		ReadFluentMetadata(endpoint, invocation, semanticModel);

		return endpoint;
	}

	/// <summary>
	/// Resolves the route prefix contributed by any enclosing `MapGroup` calls.
	/// </summary>
	/// <remarks>
	/// A group is usually captured into a local (`var g = app.MapGroup("/x"); g.MapPost(…)`),
	/// so this is a dataflow question rather than a syntactic one: the receiver is traced
	/// back through its declaration until it reaches something that is not a `MapGroup`.
	/// </remarks>
	private static string ResolveGroupPrefix(ExpressionSyntax receiver, SemanticModel semanticModel)
	{
		var prefixes = new List<string>();
		var current = receiver;
		var guard = 0;

		while (current is not null && guard++ < 16)
		{
			// Chained directly: `app.MapGroup("/x").MapPost(…)`
			if (current is InvocationExpressionSyntax
				{
					Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "MapGroup" } inner
				} invocation)
			{
				if (invocation.ArgumentList.Arguments.Count > 0 &&
					semanticModel.GetConstantValue(invocation.ArgumentList.Arguments[0].Expression).Value is string groupTemplate)
				{
					prefixes.Insert(0, groupTemplate);
				}

				current = inner.Expression;
				continue;
			}

			// Captured into a local: trace back to the initializer.
			if (current is IdentifierNameSyntax identifier)
			{
				var initializer = GetLocalInitializer(identifier, semanticModel);
				if (initializer is null) break;

				current = initializer;
				continue;
			}

			break;
		}

		return string.Concat(prefixes);
	}

	private static ExpressionSyntax? GetLocalInitializer(IdentifierNameSyntax identifier, SemanticModel semanticModel)
	{
		if (semanticModel.GetSymbolInfo(identifier).Symbol is not ILocalSymbol local) return null;

		foreach (var reference in local.DeclaringSyntaxReferences)
		{
			if (reference.GetSyntax() is VariableDeclaratorSyntax { Initializer.Value: var value })
				return value;
		}

		return null;
	}

	private static string Combine(string prefix, string template)
	{
		var combined = $"{prefix}/{template.TrimStart('/')}".Replace("//", "/");

		if (!combined.StartsWith("/"))
			combined = "/" + combined;

		return combined.Length > 1 ? combined.TrimEnd('/') : combined;
	}

	private static void AddHandler(EndpointInfo endpoint, ExpressionSyntax handler, SemanticModel semanticModel)
	{
		var parameters = GetHandlerParameters(handler, semanticModel);

		foreach (var parameter in parameters)
		{
			if (EndpointDiscoveryHelpers.IsFrameworkService(parameter.Type)) continue;

			if (EndpointDiscoveryHelpers.LooksLikeBody(parameter.Type))
			{
				endpoint.RequestBodyTypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				endpoint.RequestBodyIsValidated = parameter.Type.GetAttributes()
					.Any(x => x.AttributeClass?.Name == "GenerateJsonSchemaAttribute");
				continue;
			}

			// A route segment binds the parameter regardless of whether it is optional in the
			// signature, so this decides both the location and whether it is required.
			var fromRoute = EndpointDiscoveryHelpers.RouteBinds(endpoint.Route, parameter.Name);

			endpoint.Parameters.Add(new EndpointParameterInfo
			{
				Name = parameter.Name,
				Location = fromRoute ? "path" : "query",
				Required = fromRoute || !parameter.IsOptional,
				TypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			});
		}

		endpoint.Responses.Add(new EndpointResponseInfo
		{
			StatusCode = 200,
			TypeName = GetHandlerPayloadType(handler, semanticModel),
			Description = "Success"
		});
	}

	private static IReadOnlyList<IParameterSymbol> GetHandlerParameters(ExpressionSyntax handler, SemanticModel semanticModel)
	{
		if (semanticModel.GetSymbolInfo(handler).Symbol is IMethodSymbol methodGroup)
			return methodGroup.Parameters;

		if (handler is not AnonymousFunctionExpressionSyntax lambda) return [];

		return semanticModel.GetSymbolInfo(lambda).Symbol is IMethodSymbol symbol
			? symbol.Parameters
			: [];
	}

	private static string? GetHandlerPayloadType(ExpressionSyntax handler, SemanticModel semanticModel)
	{
		if (semanticModel.GetSymbolInfo(handler).Symbol is IMethodSymbol methodGroup)
		{
			var declared = EndpointDiscoveryHelpers.GetReturnPayloadType(methodGroup);
			if (declared is not null) return declared;
		}

		if (handler is not AnonymousFunctionExpressionSyntax lambda) return null;

		if (semanticModel.GetSymbolInfo(lambda).Symbol is IMethodSymbol lambdaSymbol)
		{
			var returnType = EndpointDiscoveryHelpers.UnwrapTask(lambdaSymbol.ReturnType);
			if (!EndpointDiscoveryHelpers.IsErasedResult(returnType) &&
				returnType.SpecialType != SpecialType.System_Void)
			{
				return returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			}
		}

		return EndpointDiscoveryHelpers.GetPayloadTypeFromBody(lambda.Body, semanticModel);
	}

	/// <summary>
	/// Reads metadata contributed by fluent calls chained onto the registration.
	/// </summary>
	private static void ReadFluentMetadata(EndpointInfo endpoint, InvocationExpressionSyntax invocation, SemanticModel semanticModel)
	{
		// Walk outward: `app.MapPost(…).WithName("x").Produces<T>(201)`
		SyntaxNode? current = invocation;

		while (current?.Parent is MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax outer } access)
		{
			switch (access.Name.Identifier.ValueText)
			{
				case "WithName":
					if (outer.ArgumentList.Arguments.Count > 0 &&
						semanticModel.GetConstantValue(outer.ArgumentList.Arguments[0].Expression).Value is string name)
					{
						endpoint.OperationId = name;
					}
					break;
				case "Produces":
					ReadProduces(endpoint, access, outer, semanticModel);
					break;
			}

			current = outer;
		}
	}

	private static void ReadProduces(
		EndpointInfo endpoint,
		MemberAccessExpressionSyntax access,
		InvocationExpressionSyntax invocation,
		SemanticModel semanticModel)
	{
		var statusCode = 200;
		if (invocation.ArgumentList.Arguments.Count > 0 &&
			semanticModel.GetConstantValue(invocation.ArgumentList.Arguments[0].Expression).Value is int code)
		{
			statusCode = code;
		}

		string? typeName = null;
		if (access.Name is GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } generic &&
			semanticModel.GetTypeInfo(generic.TypeArgumentList.Arguments[0]).Type is { } type)
		{
			typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}

		endpoint.Responses.RemoveAll(x => x.StatusCode == statusCode);
		endpoint.Responses.Add(new EndpointResponseInfo
		{
			StatusCode = statusCode,
			TypeName = typeName,
			Description = EndpointDiscoveryHelpers.DescribeStatus(statusCode)
		});
	}
}
