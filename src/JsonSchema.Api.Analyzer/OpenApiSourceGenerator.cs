using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Json.Schema.Generation.Serialization;
using Json.Schema.Generation.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = Json.Schema.Generation.SourceGeneration.TypeInfo;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// Emits this assembly's contribution to an OpenAPI document.
/// </summary>
/// <remarks>
/// Each project that references the package emits its own fragment, in the same way the
/// schema generator emits a `GeneratedJsonSchemas` class per project.  `AddOpenApi()`
/// assembles the fragments it can see into one document at startup, so controllers in a
/// referenced class library are included without that library knowing about the web host.
/// </remarks>
[Generator]
public class OpenApiSourceGenerator : IIncrementalGenerator
{
	internal const string FragmentClassName = "GeneratedOpenApiFragment";

	private const string _generateJsonSchemaAttributeName = "Json.Schema.Generation.Serialization.GenerateJsonSchemaAttribute";
	private const string _apiControllerAttributeName = "Microsoft.AspNetCore.Mvc.ApiControllerAttribute";

	/// <summary>
	/// Initializes the incremental generator.
	/// </summary>
	/// <param name="context">The initialization context.</param>
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var buildOptions = context.AnalyzerConfigOptionsProvider
			.Select(static (provider, _) =>
			{
				provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
				provider.GlobalOptions.TryGetValue("build_property.JsonSchemaDefaultEnumFormat", out var enumFormatRaw);

				var enumFormat = Enum.TryParse<EnumFormat>(enumFormatRaw, ignoreCase: true, out var parsed)
					? parsed
					: EnumFormat.Names;

				return (RootNamespace: rootNamespace ?? string.Empty, EnumFormat: enumFormat);
			});

		var schemaTypes = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				_generateJsonSchemaAttributeName,
				static (node, _) => node is TypeDeclarationSyntax,
				static (ctx, _) => ctx.TargetSymbol as INamedTypeSymbol)
			.Where(static x => x is not null)
			.Collect();

		var controllers = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				_apiControllerAttributeName,
				static (node, _) => node is ClassDeclarationSyntax,
				static (ctx, _) => ctx.TargetSymbol as INamedTypeSymbol)
			.Where(static x => x is not null)
			.Collect();

		var minimalApis = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (node, _) => MinimalApiDiscovery.CouldBeRegistration(node),
				static (ctx, _) => MinimalApiDiscovery.Discover((InvocationExpressionSyntax)ctx.Node, ctx.SemanticModel))
			.Where(static x => x is not null)
			.Collect();

		var source = context.CompilationProvider
			.Combine(schemaTypes)
			.Combine(controllers)
			.Combine(minimalApis)
			.Combine(buildOptions);

		context.RegisterSourceOutput(source, static (spc, values) =>
			Execute(
				values.Left.Left.Left.Left,
				values.Left.Left.Left.Right,
				values.Left.Left.Right,
				values.Left.Right,
				values.Right.RootNamespace,
				values.Right.EnumFormat,
				spc));
	}

	private static void Execute(
		Compilation compilation,
		ImmutableArray<INamedTypeSymbol?> schemaTypeSymbols,
		ImmutableArray<INamedTypeSymbol?> controllerSymbols,
		ImmutableArray<EndpointInfo?> minimalApiEndpoints,
		string rootNamespace,
		EnumFormat enumFormat,
		SourceProductionContext context)
	{
		var endpoints = new List<EndpointInfo>();

		foreach (var controller in controllerSymbols)
		{
			if (controller is null) continue;
			endpoints.AddRange(ControllerDiscovery.Discover(controller));
		}

		foreach (var endpoint in minimalApiEndpoints)
		{
			if (endpoint is null) continue;
			endpoints.Add(endpoint);
		}

		var types = new List<TypeInfo>();
		foreach (var symbol in schemaTypeSymbols)
		{
			if (symbol is null) continue;

			var typeInfo = TypeAnalyzer.Analyze(
				compilation,
				symbol,
				symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == "GenerateJsonSchemaAttribute"),
				context.ReportDiagnostic,
				NamingConvention.CamelCase,
				Json.Schema.Generation.PropertyOrder.AsDeclared,
				enumFormat);

			if (typeInfo is not null)
				types.Add(typeInfo);
		}

		var referencedFragments = ReferencedFragments.Find(compilation);

		if (types.Count == 0 && endpoints.Count == 0 && referencedFragments.Count == 0) return;

		var source = FragmentEmitter.Emit(types, endpoints, rootNamespace, referencedFragments, enumFormat);

		context.AddSource($"{FragmentClassName}.g.cs", source);
	}
}
