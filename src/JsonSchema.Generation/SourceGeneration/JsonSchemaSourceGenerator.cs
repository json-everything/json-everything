using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Json.Schema.Generation.Serialization;
using Json.Schema.Generation.SourceGeneration.Emitters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
#pragma warning disable RS1041

namespace Json.Schema.Generation.SourceGeneration;

/// <summary>
/// Source generator that creates static JSON schemas for types decorated with [GenerateJsonSchema].
/// </summary>
[Generator]
public class JsonSchemaSourceGenerator : IIncrementalGenerator
{
	private const string _generateJsonSchemaAttributeName = "Json.Schema.Generation.Serialization.GenerateJsonSchemaAttribute";
	private const string _schemaHandlerAttributeName = "Json.Schema.Generation.SchemaHandlerAttribute";
	private const string _idAttributeName = "Json.Schema.Generation.Serialization.IdAttribute";

	/// <summary>
	/// Initializes the incremental generator.
	/// </summary>
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var generationOptions = context.AnalyzerConfigOptionsProvider
			.Select(static (provider, _) =>
			{
				provider.GlobalOptions.TryGetValue("build_property.DisableJsonSchemaSourceGeneration", out var disabled);
				provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
				provider.GlobalOptions.TryGetValue("build_property.JsonSchemaDefaultPropertyNaming", out var namingRaw);
				provider.GlobalOptions.TryGetValue("build_property.JsonSchemaDefaultPropertyOrder", out var orderRaw);

				var defaultNaming = Enum.TryParse<NamingConvention>(namingRaw, ignoreCase: true, out var parsedNaming)
					? parsedNaming
					: NamingConvention.AsDeclared;
				var defaultOrder = Enum.TryParse<PropertyOrder>(orderRaw, ignoreCase: true, out var parsedOrder)
					? parsedOrder
					: PropertyOrder.AsDeclared;

				return new GenerationOptions
				{
					IsDisabled = disabled?.Equals("true", StringComparison.OrdinalIgnoreCase) == true,
					RootNamespace = rootNamespace ?? string.Empty,
					DefaultPropertyNaming = defaultNaming,
					DefaultPropertyOrder = defaultOrder
				};
			});

		var typesWithAttribute = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				_generateJsonSchemaAttributeName,
				static (node, _) => node is TypeDeclarationSyntax,
				static (ctx, _) => GetTypeToGenerate(ctx))
			.Where(static type => type is not null);

		var compilationAndTypesAndOptions = context.CompilationProvider
			.Combine(typesWithAttribute.Collect())
			.Combine(generationOptions);

		context.RegisterSourceOutput(compilationAndTypesAndOptions, static (spc, source) =>
		{
			var generationOptions = source.Right;
			if (generationOptions.IsDisabled) return;

			Execute(source.Left.Left, source.Left.Right, generationOptions, spc);
		});
	}

	private static TypeToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context)
	{
		if (context.TargetSymbol is not INamedTypeSymbol typeSymbol) return null;

		var attributeData = context.Attributes.FirstOrDefault();
		if (attributeData == null) return null;

		return new TypeToGenerate(typeSymbol, attributeData);
	}

	private static void Execute(Compilation compilation, ImmutableArray<TypeToGenerate?> types, GenerationOptions options, SourceProductionContext context)
	{
		if (types.IsDefaultOrEmpty) return;

		var validTypes = types.Where(t => t != null).Select(t => t!).ToList();
		if (validTypes.Count == 0) return;

		var selfGeneratingAssemblies = FindSelfGeneratingAssemblies(compilation);
		var generatedSchemaMembersByAssembly = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
		var foreignTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
		var analyzedTypes = new List<TypeInfo>();
		var allEncounteredTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
		var discoveredTypeOptions = new Dictionary<ITypeSymbol, (NamingConvention Naming, PropertyOrder Order)>(SymbolEqualityComparer.Default);
		foreach (var type in validTypes)
		{
			var typeInfo = TypeAnalyzer.Analyze(compilation, type.TypeSymbol, type.AttributeData, context.ReportDiagnostic, options.DefaultPropertyNaming, options.DefaultPropertyOrder);
			if (typeInfo != null) 
			{
				analyzedTypes.Add(typeInfo);
				RegisterTypeOptions(discoveredTypeOptions, typeInfo.TypeSymbol, typeInfo.PropertyNaming, typeInfo.PropertyOrder);
				CollectAllTypes(compilation, typeInfo, allEncounteredTypes, selfGeneratingAssemblies, generatedSchemaMembersByAssembly, foreignTypes, context.ReportDiagnostic, discoveredTypeOptions, typeInfo.PropertyNaming, typeInfo.PropertyOrder);
			}
		}

		if (analyzedTypes.Count == 0) return;

		var schemaHandlers = DiscoverSchemaHandlers(compilation);

		// Analyze all encountered types that aren't already analyzed
		var allTypeInfos = new List<TypeInfo>(analyzedTypes);
		foreach (var typeSymbol in allEncounteredTypes)
		{
			if (analyzedTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.TypeSymbol, typeSymbol))) continue;

			if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
			{
				if (allTypeInfos.Any(t => SymbolEqualityComparer.Default.Equals(t.TypeSymbol, arrayTypeSymbol))) continue;

				allTypeInfos.Add(AnalyzeArrayType(arrayTypeSymbol, options.DefaultPropertyNaming, options.DefaultPropertyOrder));
				continue;
			}

			if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
			{
				var naming = options.DefaultPropertyNaming;
				var order = options.DefaultPropertyOrder;
				if (discoveredTypeOptions.TryGetValue(namedTypeSymbol, out var discovered))
				{
					naming = discovered.Naming;
					order = discovered.Order;
				}

				var typeInfo = TypeAnalyzer.Analyze(compilation, namedTypeSymbol, null, context.ReportDiagnostic, naming, order);
				if (typeInfo != null)
					allTypeInfos.Add(typeInfo);
			}
		}

		ResolvePropertyNameConflicts(allTypeInfos, options.RootNamespace);

		var foreignTypeEntries = new List<(string TypeName, string SchemaId)>();
		foreach (var ft in foreignTypes)
			foreignTypeEntries.Add((ft.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), GetForeignSchemaId(ft)));

		var targetNamespace = options.RootNamespace;
		var classDeclaration = DetectGeneratedJsonSchemasClass(compilation, targetNamespace, context.ReportDiagnostic);
		var generatedCode = SchemaCodeEmitter.EmitGeneratedClass(allTypeInfos, targetNamespace, classDeclaration, schemaHandlers, foreignTypeEntries);

		context.AddSource("GeneratedJsonSchemas.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
	}

	private static List<SchemaHandlerInfo> DiscoverSchemaHandlers(Compilation compilation)
	{
		var results = new List<SchemaHandlerInfo>();
		var systemType = compilation.GetTypeByMetadataName("System.Type");
		CollectSchemaHandlers(compilation.Assembly.GlobalNamespace, results, systemType);
		return results;
	}

	private static void CollectSchemaHandlers(INamespaceSymbol namespaceSymbol, List<SchemaHandlerInfo> results, INamedTypeSymbol? systemType)
	{
		foreach (var type in namespaceSymbol.GetTypeMembers())
			CollectSchemaHandlers(type, results, systemType);

		foreach (var nested in namespaceSymbol.GetNamespaceMembers())
			CollectSchemaHandlers(nested, results, systemType);
	}

	private static void CollectSchemaHandlers(INamedTypeSymbol typeSymbol, List<SchemaHandlerInfo> results, INamedTypeSymbol? systemType)
	{
		foreach (var attr in typeSymbol.GetAttributes())
		{
			if (attr.AttributeClass?.ToDisplayString() != _schemaHandlerAttributeName) continue;
			if (attr.ConstructorArguments.Length == 0) continue;
			if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol targetTypeSymbol) continue;

			var applyMethod = typeSymbol.GetMembers("Apply")
				.OfType<IMethodSymbol>()
				.FirstOrDefault(m => m is { IsStatic: true } &&
				                     m.Parameters.Length == 2 &&
				                     m.Parameters[0].Type.Name == "JsonSchemaBuilder" &&
				                     SymbolEqualityComparer.Default.Equals(m.Parameters[1].Type, systemType) &&
				                     (m.ReturnsVoid || m.ReturnType.Name == "JsonSchemaBuilder"));

			if (applyMethod == null) continue;

			var isOpenGenericTarget = targetTypeSymbol.IsUnboundGenericType;

			results.Add(new SchemaHandlerInfo
			{
				HandlerTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				TargetTypeName = targetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				IsOpenGenericTarget = isOpenGenericTarget,
				ReturnsBuilder = !applyMethod.ReturnsVoid
			});
		}

		foreach (var nested in typeSymbol.GetTypeMembers())
			CollectSchemaHandlers(nested, results, systemType);
	}

	private static void CollectAllTypes(Compilation compilation, TypeInfo typeInfo, HashSet<ITypeSymbol> allTypes, HashSet<IAssemblySymbol> selfGeneratingAssemblies, Dictionary<string, HashSet<string>> generatedSchemaMembersByAssembly, HashSet<ITypeSymbol> foreignTypes, Action<Diagnostic> reportDiagnostic, Dictionary<ITypeSymbol, (NamingConvention Naming, PropertyOrder Order)> discoveredTypeOptions, NamingConvention naming, PropertyOrder order)
	{
		CollectTypeRecursive(compilation, typeInfo.TypeSymbol, allTypes, selfGeneratingAssemblies, generatedSchemaMembersByAssembly, foreignTypes, reportDiagnostic, discoveredTypeOptions, naming, order);

		foreach (var prop in typeInfo.Properties)
		{
			CollectTypeRecursive(compilation, prop.Type, allTypes, selfGeneratingAssemblies, generatedSchemaMembersByAssembly, foreignTypes, reportDiagnostic, discoveredTypeOptions, naming, order);
		}
	}

	private static void CollectTypeRecursive(Compilation compilation, ITypeSymbol typeSymbol, HashSet<ITypeSymbol> allTypes, HashSet<IAssemblySymbol> selfGeneratingAssemblies, Dictionary<string, HashSet<string>> generatedSchemaMembersByAssembly, HashSet<ITypeSymbol> foreignTypes, Action<Diagnostic> reportDiagnostic, Dictionary<ITypeSymbol, (NamingConvention Naming, PropertyOrder Order)> discoveredTypeOptions, NamingConvention naming, PropertyOrder order)
	{
		var unwrapped = CodeEmitterHelpers.UnwrapNullable(typeSymbol);
		if (IsBuiltInJsonDomType(unwrapped)) return;

		var typeKind = SchemaCodeEmitter.DetermineTypeKind(unwrapped);
		if (typeKind is TypeKind.Boolean or TypeKind.Integer or 
		    TypeKind.Number or TypeKind.String or 
		    TypeKind.DateTime or TypeKind.Guid or 
		    TypeKind.Uri)
			return;

		if (typeKind == TypeKind.Enum && unwrapped is INamedTypeSymbol enumType)
		{
			if (IsTypeCoveredBySelfGeneratingAssemblies(enumType, selfGeneratingAssemblies, generatedSchemaMembersByAssembly))
				foreignTypes.Add(enumType);
			else if (allTypes.Add(enumType))
				RegisterTypeOptions(discoveredTypeOptions, enumType, naming, order);
			return;
		}

		if (typeKind == TypeKind.Array)
		{
			allTypes.Add(unwrapped);

			var elementType = CodeEmitterHelpers.GetElementType(unwrapped);
			if (elementType != null)
				CollectTypeRecursive(compilation, elementType, allTypes, selfGeneratingAssemblies, generatedSchemaMembersByAssembly, foreignTypes, reportDiagnostic, discoveredTypeOptions, naming, order);
			return;
		}

		if (typeKind == TypeKind.Dictionary)
		{
			allTypes.Add(unwrapped);

			var keyType = CodeEmitterHelpers.GetDictionaryKeyType(unwrapped);
			if (keyType != null)
				CollectTypeRecursive(compilation, keyType, allTypes, selfGeneratingAssemblies, generatedSchemaMembersByAssembly, foreignTypes, reportDiagnostic, discoveredTypeOptions, naming, order);

			var valueType = CodeEmitterHelpers.GetDictionaryValueType(unwrapped);
			if (valueType != null)
				CollectTypeRecursive(compilation, valueType, allTypes, selfGeneratingAssemblies, generatedSchemaMembersByAssembly, foreignTypes, reportDiagnostic, discoveredTypeOptions, naming, order);
			return;
		}

		if (typeKind == TypeKind.Object && unwrapped is INamedTypeSymbol namedType)
		{
			if (IsTypeCoveredBySelfGeneratingAssemblies(namedType, selfGeneratingAssemblies, generatedSchemaMembersByAssembly))
			{
				foreignTypes.Add(namedType);
				return;
			}

			RegisterTypeOptions(discoveredTypeOptions, namedType, naming, order);

			if (allTypes.Add(namedType))
			{
				// Analyze the type to collect its properties' types
				var tempTypeInfo = TypeAnalyzer.Analyze(compilation, namedType, null, reportDiagnostic, naming, order);
				if (tempTypeInfo != null)
				{
					foreach (var prop in tempTypeInfo.Properties)
					{
						CollectTypeRecursive(compilation, prop.Type, allTypes, selfGeneratingAssemblies, generatedSchemaMembersByAssembly, foreignTypes, reportDiagnostic, discoveredTypeOptions, naming, order);
					}
				}
			}
		}
	}

	private static bool IsBuiltInJsonDomType(ITypeSymbol typeSymbol)
	{
		var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		return typeName is
			"global::System.Text.Json.JsonDocument" or
			"global::System.Text.Json.JsonElement" or
			"global::System.Text.Json.Nodes.JsonNode" or
			"global::System.Text.Json.Nodes.JsonValue" or
			"global::System.Text.Json.Nodes.JsonObject" or
			"global::System.Text.Json.Nodes.JsonArray";
	}

	private static void RegisterTypeOptions(Dictionary<ITypeSymbol, (NamingConvention Naming, PropertyOrder Order)> discoveredTypeOptions, ITypeSymbol typeSymbol, NamingConvention naming, PropertyOrder order)
	{
		if (discoveredTypeOptions.ContainsKey(typeSymbol)) return;

		discoveredTypeOptions[typeSymbol] = (naming, order);
	}

	private static TypeInfo AnalyzeArrayType(IArrayTypeSymbol typeSymbol, NamingConvention propertyNaming, PropertyOrder propertyOrder)
	{
		return new TypeInfo
		{
			TypeSymbol = typeSymbol,
			FullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
			SchemaPropertyName = GetSchemaPropertyName(typeSymbol),
			PropertyNaming = propertyNaming,
			PropertyOrder = propertyOrder,
			StrictConditionals = false,
			Kind = TypeKind.Array,
			IsNullable = false
		};
	}

	private static HashSet<IAssemblySymbol> FindSelfGeneratingAssemblies(Compilation compilation)
	{
		var result = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
		foreach (var reference in compilation.References)
		{
			if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
			{
				if (HasGeneratedJsonSchemasClass(assembly.GlobalNamespace) || ReferencesSchemaGenerationPackage(assembly))
					result.Add(assembly);
			}
		}
		return result;
	}

	private static bool IsTypeCoveredBySelfGeneratingAssemblies(INamedTypeSymbol typeSymbol, HashSet<IAssemblySymbol> selfGeneratingAssemblies, Dictionary<string, HashSet<string>> generatedSchemaMembersByAssembly)
	{
		var propertyName = GetSchemaPropertyName(typeSymbol);
		foreach (var assembly in selfGeneratingAssemblies)
		{
			var isDeclaringAssembly = SymbolEqualityComparer.Default.Equals(assembly, typeSymbol.ContainingAssembly);
			if (!isDeclaringAssembly && !ReferencesAssembly(assembly, typeSymbol.ContainingAssembly)) continue;

			var memberNames = GetGeneratedSchemaMemberNames(assembly, generatedSchemaMembersByAssembly);
			if (memberNames.Contains(propertyName))
				return true;
		}

		return false;
	}

	private static bool ReferencesAssembly(IAssemblySymbol sourceAssembly, IAssemblySymbol targetAssembly)
	{
		foreach (var module in sourceAssembly.Modules)
		{
			foreach (var referencedAssembly in module.ReferencedAssemblySymbols)
			{
				if (SymbolEqualityComparer.Default.Equals(referencedAssembly, targetAssembly))
					return true;
			}
		}

		return false;
	}

	private static HashSet<string> GetGeneratedSchemaMemberNames(IAssemblySymbol assembly, Dictionary<string, HashSet<string>> generatedSchemaMembersByAssembly)
	{
		var key = assembly.Identity.ToString();
		if (generatedSchemaMembersByAssembly.TryGetValue(key, out var existing))
			return existing;

		var names = new HashSet<string>(StringComparer.Ordinal);
		foreach (var schemaType in GetGeneratedJsonSchemasTypes(assembly.GlobalNamespace))
		{
			foreach (var member in schemaType.GetMembers())
			{
				if (member is IFieldSymbol { IsStatic: true, Type.Name: "JsonSchema" } field)
					names.Add(field.Name);
				if (member is IPropertySymbol { IsStatic: true, Type.Name: "JsonSchema" } prop)
					names.Add(prop.Name);
			}
		}

		generatedSchemaMembersByAssembly[key] = names;
		return names;
	}

	private static IEnumerable<INamedTypeSymbol> GetGeneratedJsonSchemasTypes(INamespaceSymbol ns)
	{
		foreach (var type in ns.GetTypeMembers("GeneratedJsonSchemas"))
			yield return type;

		foreach (var nested in ns.GetNamespaceMembers())
		{
			foreach (var type in GetGeneratedJsonSchemasTypes(nested))
				yield return type;
		}
	}

	private static string GetSchemaPropertyName(INamedTypeSymbol typeSymbol)
	{
		var currentName = typeSymbol.Name;

		if (typeSymbol.IsGenericType && typeSymbol.TypeArguments.Length > 0)
		{
			var typeArgNames = typeSymbol.TypeArguments.Select(GetTypeArgumentPropertyName);
			currentName = $"{currentName}Of{string.Join("And", typeArgNames)}";
		}

		return typeSymbol.ContainingType != null
			? $"{GetSchemaPropertyName(typeSymbol.ContainingType)}_{currentName}"
			: currentName;
	}

	private static string GetSchemaPropertyName(IArrayTypeSymbol typeSymbol)
	{
		return $"{GetTypeArgumentPropertyName(typeSymbol.ElementType)}Array";
	}

	private static string GetTypeArgumentPropertyName(ITypeSymbol typeSymbol)
	{
		var isNullable = typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
		                 typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
		var unwrapped = CodeEmitterHelpers.UnwrapNullable(typeSymbol);

		string name;
		if (unwrapped is INamedTypeSymbol namedType)
			name = GetSchemaPropertyName(namedType);
		else if (unwrapped is IArrayTypeSymbol arrayType)
			name = $"{GetTypeArgumentPropertyName(arrayType.ElementType)}Array";
		else
			name = SanitizeSchemaPropertyName(unwrapped.Name);

		if (isNullable)
			name += "Nullable";

		return name;
	}

	private static string SanitizeSchemaPropertyName(string name)
	{
		var sb = new StringBuilder(name.Length);
		var lastWasUnderscore = false;

		foreach (var ch in name)
		{
			if (char.IsLetterOrDigit(ch) || ch == '_')
			{
				sb.Append(ch);
				lastWasUnderscore = false;
				continue;
			}

			if (lastWasUnderscore) continue;

			sb.Append('_');
			lastWasUnderscore = true;
		}

		return sb.ToString().Trim('_');
	}

	private static bool ReferencesSchemaGenerationPackage(IAssemblySymbol assembly)
	{
		foreach (var module in assembly.Modules)
		{
			foreach (var referencedAssembly in module.ReferencedAssemblySymbols)
			{
				if (referencedAssembly.Name is "JsonSchema.Net.Generation" or "Json.Schema.Generation")
					return true;
			}
		}

		return false;
	}

	private static bool HasGeneratedJsonSchemasClass(INamespaceSymbol ns)
	{
		if (ns.GetTypeMembers("GeneratedJsonSchemas").Length > 0)
			return true;

		foreach (var nested in ns.GetNamespaceMembers())
		{
			if (HasGeneratedJsonSchemasClass(nested))
				return true;
		}

		return false;
	}

	private static string GetForeignSchemaId(ITypeSymbol typeSymbol)
	{
		var idAttr = typeSymbol.GetAttributes()
			.FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == _idAttributeName);
		if (idAttr != null && idAttr.ConstructorArguments.Length > 0 &&
		    idAttr.ConstructorArguments[0].Value is string idStr)
			return idStr;
		return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}

	private static ClassDeclarationInfo DetectGeneratedJsonSchemasClass(
		Compilation compilation, 
		string namespaceName,
		Action<Diagnostic> reportDiagnostic)
	{
		// Search for GeneratedJsonSchemas class in this namespace
		var namespaceSymbol = string.IsNullOrEmpty(namespaceName) 
			? compilation.GlobalNamespace 
			: GetNamespaceSymbol(compilation.GlobalNamespace, namespaceName);

		if (namespaceSymbol == null)
			return ClassDeclarationInfo.Default;

		var classSymbol = namespaceSymbol.GetTypeMembers("GeneratedJsonSchemas").FirstOrDefault();
		
		if (classSymbol == null)
			return ClassDeclarationInfo.Default;

		// Check if the class is partial
		var isPartial = classSymbol.DeclaringSyntaxReferences
			.Select(r => r.GetSyntax())
			.OfType<ClassDeclarationSyntax>()
			.Any(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

		if (!isPartial)
		{
			var namespacePart = string.IsNullOrEmpty(namespaceName) 
				? "the global namespace" 
				: $"namespace '{namespaceName}'";
			var diagnostic = Diagnostic.Create(
				Diagnostics.GeneratedJsonSchemasClassMustBePartial,
				classSymbol.Locations.FirstOrDefault(),
				namespacePart);
			reportDiagnostic(diagnostic);
			
			// Still return info but it will cause compilation error
			return ClassDeclarationInfo.Default;
		}

		// Extract modifiers
		var isPublic = classSymbol.DeclaredAccessibility == Accessibility.Public;
		var isInternal = classSymbol.DeclaredAccessibility == Accessibility.Internal;
		var isStatic = classSymbol.IsStatic;

		return new ClassDeclarationInfo
		{
			IsPublic = isPublic,
			IsInternal = isInternal,
			IsStatic = isStatic,
			IsPartial = true
		};
	}

	private static INamespaceSymbol? GetNamespaceSymbol(INamespaceSymbol rootNamespace, string qualifiedName)
	{
		var parts = qualifiedName.Split('.');
		var current = rootNamespace;

		foreach (var part in parts)
		{
			var next = current.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == part);
			if (next == null)
				return null;
			current = next;
		}

		return current;
	}

	private static void ResolvePropertyNameConflicts(List<TypeInfo> types, string rootNamespace)
	{
		var groups = types.GroupBy(t => t.SchemaPropertyName).Where(g => g.Count() > 1);
		foreach (var group in groups)
		{
			foreach (var type in group)
			{
				var relNs = GetRelativeNamespace(type.TypeSymbol, rootNamespace);
				var prefix = string.IsNullOrEmpty(relNs) ? string.Empty : relNs + "_";
				type.ResolvedPropertyName = prefix + type.SchemaPropertyName;
			}
		}

		var usedNames = new HashSet<string>(StringComparer.Ordinal);
		foreach (var type in types.OrderBy(t => t.FullyQualifiedName, StringComparer.Ordinal))
		{
			var baseName = type.ResolvedPropertyName ?? type.SchemaPropertyName;
			var candidate = baseName;
			var index = 2;

			while (!usedNames.Add(candidate))
			{
				candidate = $"{baseName}_{index}";
				index++;
			}

			type.ResolvedPropertyName = candidate == type.SchemaPropertyName ? null : candidate;
		}
	}

	private static string GetRelativeNamespace(ITypeSymbol typeSymbol, string rootNamespace)
	{
		var ns = GetContainingNamespace(typeSymbol);
		if (!string.IsNullOrEmpty(rootNamespace) && ns.StartsWith(rootNamespace))
			ns = ns.Length > rootNamespace.Length ? ns[(rootNamespace.Length + 1)..] : string.Empty;
		return ns.Replace(".", "_");
	}

	private static string GetContainingNamespace(ITypeSymbol typeSymbol)
	{
		var ns = typeSymbol.ContainingNamespace;
		if (ns == null || ns.IsGlobalNamespace) return string.Empty;
		var parts = new List<string>();
		while (ns is { IsGlobalNamespace: false })
		{
			parts.Add(ns.Name);
			ns = ns.ContainingNamespace;
		}
		parts.Reverse();
		return string.Join(".", parts);
	}

	private sealed class TypeToGenerate
	{
		public INamedTypeSymbol TypeSymbol { get; }
		public AttributeData AttributeData { get; }

		public TypeToGenerate(INamedTypeSymbol typeSymbol, AttributeData attributeData)
		{
			TypeSymbol = typeSymbol;
			AttributeData = attributeData;
		}
	}

	private sealed class GenerationOptions
	{
		public required bool IsDisabled { get; init; }
		public required string RootNamespace { get; init; }
		public required NamingConvention DefaultPropertyNaming { get; init; }
		public required PropertyOrder DefaultPropertyOrder { get; init; }
	}
}
