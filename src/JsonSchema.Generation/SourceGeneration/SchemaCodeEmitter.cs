using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Json.Schema.Generation.SourceGeneration.Emitters;
using Json.Schema.Generation.Serialization;
using Microsoft.CodeAnalysis;

namespace Json.Schema.Generation.SourceGeneration;

internal static class SchemaCodeEmitter
{
	public static void EmitSchemaForType(StringBuilder sb, ITypeSymbol typeSymbol, bool isNullable, string indent, SchemaEmissionContext? context = null, List<AttributeInfo>? itemAttributes = null, List<AttributeInfo>? propertyAttributes = null)
	{
		var unwrapped = CodeEmitterHelpers.UnwrapNullable(typeSymbol);
		var typeKind = DetermineTypeKind(unwrapped);
		if (typeKind == TypeKind.Object && context != null)
		{
			if (context.HasSchemaHandler(unwrapped))
			{
				sb.AppendLine();

				if (isNullable)
				{
					sb.Append($"{indent}.AnyOf(");
					sb.AppendLine();
					sb.Append($"{indent}\tnew JsonSchemaBuilder().BuildForType(typeof({unwrapped.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})),");
					sb.AppendLine();
					sb.Append($"{indent}\tnew JsonSchemaBuilder().Type(SchemaValueType.Null)");
					sb.AppendLine();
					sb.Append($"{indent})");
				}
				else
					sb.Append($"{indent}.BuildForType(typeof({unwrapped.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}))");

				return;
			}

			if (context.RootType != null && SymbolEqualityComparer.Default.Equals(unwrapped, CodeEmitterHelpers.UnwrapNullable(context.RootType)))
				return;

			var refUri = context.GetRefUri(unwrapped);
			if (string.IsNullOrEmpty(refUri))
			{
				sb.AppendLine();
				if (isNullable)
					sb.Append($"{indent}.Type(SchemaValueType.Object, SchemaValueType.Null)");
				else
					sb.Append($"{indent}.Type(SchemaValueType.Object)");
				return;
			}

			sb.AppendLine();
			
			if (isNullable)
			{
				sb.Append($"{indent}.AnyOf(");
				sb.AppendLine();
				sb.Append($"{indent}\tnew JsonSchemaBuilder().Ref(\"{refUri}\"),");
				sb.AppendLine();
				sb.Append($"{indent}\tnew JsonSchemaBuilder().Type(SchemaValueType.Null)");
				sb.AppendLine();
				sb.Append($"{indent})");
			}
			else
				sb.Append($"{indent}.Ref(\"{refUri}\")");

			return;
		}

		if (typeKind == TypeKind.Array)
		{
			sb.AppendLine();
			if (isNullable)
				sb.Append($"{indent}.Type(SchemaValueType.Array, SchemaValueType.Null)");
			else
				sb.Append($"{indent}.Type(SchemaValueType.Array)");

			var elementType = CodeEmitterHelpers.GetElementType(unwrapped);
			if (elementType != null)
			{
				sb.AppendLine();
				sb.Append($"{indent}.Items(");
				sb.Append("new JsonSchemaBuilder()");

				EmitSchemaForType(sb, elementType, false, indent + "\t", context);

				if (itemAttributes is { Count: > 0 })
					EmitAttributes(sb, itemAttributes, indent + "\t");

				sb.Append(")");
			}

			return;
		}

		if (typeKind == TypeKind.Enum && context != null)
		{
			if (context.RootType != null && SymbolEqualityComparer.Default.Equals(unwrapped, CodeEmitterHelpers.UnwrapNullable(context.RootType)))
				return;

			var refUri = context.GetRefUri(unwrapped);
			if (!string.IsNullOrEmpty(refUri))
			{
				sb.AppendLine();

				if (isNullable)
				{
					sb.Append($"{indent}.AnyOf(");
					sb.AppendLine();
					sb.Append($"{indent}\tnew JsonSchemaBuilder().Ref(\"{refUri}\"),");
					sb.AppendLine();
					sb.Append($"{indent}\tnew JsonSchemaBuilder().Type(SchemaValueType.Null)");
					sb.AppendLine();
					sb.Append($"{indent})");
				}
				else
					sb.Append($"{indent}.Ref(\"{refUri}\")");

				return;
			}
		}
		
		if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
		{
			var typeInfo = new TypeInfo
			{
				TypeSymbol = namedTypeSymbol,
				FullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				SchemaPropertyName = "",
				PropertyNaming = NamingConvention.AsDeclared,
				PropertyOrder = PropertyOrder.AsDeclared,
				StrictConditionals = false,
				Kind = typeKind,
				IsNullable = isNullable,
				ItemAttributes = itemAttributes,
				PropertyAttributes = propertyAttributes
			};

			if (typeKind == TypeKind.Enum)
			{
				foreach (var member in namedTypeSymbol.GetMembers())
				{
					if (member is IFieldSymbol { IsConst: true, HasConstantValue: true } field)
					{
						if (CodeEmitterHelpers.ShouldIncludeEnumMember(field))
							typeInfo.EnumValues.Add(field.Name);
					}
				}
			}

			var emitter = SchemaEmitterRegistry.Emitters.FirstOrDefault(e => e.Handles(typeInfo));
			if (emitter != null)
				emitter.EmitSchema(sb, typeInfo, indent, context ?? new SchemaEmissionContext());
			else
			{
				sb.AppendLine();
				sb.Append($"{indent}.Type(SchemaValueType.Object)");
			}
		}
		else
		{
			sb.AppendLine();
			sb.Append($"{indent}.Type(SchemaValueType.Object)");
		}
	}

	internal static TypeKind DetermineTypeKind(ITypeSymbol typeSymbol)
	{
		var unwrappedType = CodeEmitterHelpers.UnwrapNullable(typeSymbol);
		
		switch (unwrappedType.SpecialType)
		{
			case SpecialType.System_Boolean:
				return TypeKind.Boolean;
			case SpecialType.System_Byte:
			case SpecialType.System_SByte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
			case SpecialType.System_UInt32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
				return TypeKind.Integer;
			case SpecialType.System_Single:
			case SpecialType.System_Double:
			case SpecialType.System_Decimal:
				return TypeKind.Number;
			case SpecialType.System_String:
				return TypeKind.String;
		}
		
		var typeString = unwrappedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		
		switch (typeString)
		{
			case "global::System.Text.Json.JsonDocument" or "global::System.Text.Json.JsonElement" or "global::System.Text.Json.Nodes.JsonNode" or "global::System.Text.Json.Nodes.JsonValue":
				return TypeKind.Any;
			case "global::System.Text.Json.Nodes.JsonObject":
				return TypeKind.Object;
			case "global::System.Text.Json.Nodes.JsonArray":
				return TypeKind.Array;
			case "global::System.DateTime" or "global::System.DateTimeOffset":
				return TypeKind.DateTime;
			case "global::System.Guid":
				return TypeKind.Guid;
			case "global::System.Uri":
				return TypeKind.Uri;
		}

		if (unwrappedType.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum) return TypeKind.Enum;
		if (unwrappedType is IArrayTypeSymbol || CodeEmitterHelpers.IsCollectionType(unwrappedType)) return TypeKind.Array;
		if (CodeEmitterHelpers.IsDictionaryType(unwrappedType)) return TypeKind.Dictionary;

		return TypeKind.Object;
	}

	public static string EmitGeneratedClass(List<TypeInfo> types, string namespaceName, ClassDeclarationInfo classDeclaration, List<SchemaHandlerInfo> schemaHandlers, IReadOnlyList<(string TypeName, string SchemaId)> foreignTypeEntries)
	{
		var sb = new StringBuilder();

		sb.AppendLine("// <auto-generated/>");
		sb.AppendLine("#nullable enable");
		sb.AppendLine();
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
		sb.AppendLine("using System.Runtime.CompilerServices;");
		sb.AppendLine("using System.Text.Json.Serialization;");
		sb.AppendLine("using Json.Schema;");
		sb.AppendLine("using Json.Schema.Serialization;");
		sb.AppendLine();

		if (!string.IsNullOrEmpty(namespaceName))
		{
			sb.AppendLine($"namespace {namespaceName};");
			sb.AppendLine();
		}

		var customAttributes = new Dictionary<string, AttributeInfo>();
		foreach (var type in types)
		{
			CollectCustomAttributes(type.TypeAttributes, customAttributes);
			foreach (var prop in type.Properties)
			{
				CollectCustomAttributes(prop.Attributes, customAttributes);
			}
		}

		EmitExtensionMethods(sb, customAttributes);
		sb.AppendLine();

		sb.AppendLine("/// <summary>");
		sb.AppendLine("/// Contains generated JSON schemas for types decorated with [GenerateJsonSchema].");
		sb.AppendLine("/// </summary>");
		sb.AppendLine(classDeclaration.GetDeclarationString());
		sb.AppendLine("{");

		var orderedTypes = OrderTypesTopologically(types);

		var typeIds = new Dictionary<string, string>();
		foreach (var type in orderedTypes)
		{
			var typeKey = SchemaEmissionContext.GetTypeKey(type.TypeSymbol);
			typeIds[typeKey] = GetSchemaId(type);
		}

		foreach (var (typeName, schemaId) in foreignTypeEntries)
			typeIds[typeName] = schemaId;

		var shapeAliases = GetShapeAliases(orderedTypes);
		// Map non-canonical FQN -> canonical property name for aliased shapes
		var aliasPropertyMap = shapeAliases.ToDictionary(a => a.TypeName, a => a.CanonicalPropertyName);

		foreach (var type in orderedTypes)
		{
			if (!aliasPropertyMap.ContainsKey(type.FullyQualifiedName))
				EmitSchemaProperty(sb, type, typeIds, schemaHandlers);
		}

		foreach (var type in orderedTypes)
		{
			if (aliasPropertyMap.TryGetValue(type.FullyQualifiedName, out var canonicalProp))
				EmitAliasProperty(sb, type, canonicalProp);
		}

		EmitBuildForTypeMethod(sb, orderedTypes, schemaHandlers, foreignTypeEntries, shapeAliases.Select(a => (a.TypeName, a.SchemaId)).ToList());

		EmitRegisterSchemasMethod(sb, orderedTypes, aliasPropertyMap);

		sb.AppendLine("}");
		sb.AppendLine();
		
		EmitRegistrationClass(sb, types);

		return sb.ToString();
	}

	private static string GetSchemaId(TypeInfo type)
	{
		var idAttr = type.TypeAttributes.FirstOrDefault(a => a.AttributeName == "IdAttribute");
		if (idAttr != null && idAttr.Parameters.TryGetValue("arg0", out var idValue) && idValue is string idStr)
			return idStr;

		return ToUrn(type.FullyQualifiedName);
	}

	internal static string ToUrn(string typeName)
	{
		// Remove global:: prefix if present
		if (typeName.StartsWith("global::"))
			typeName = typeName.Substring(8);

		// Canonical collection shapes share IDs.
		if (typeName.StartsWith("System.Collections.Generic.IEnumerable<") ||
			typeName.StartsWith("System.Collections.Generic.IReadOnlyCollection<") ||
			typeName.StartsWith("System.Collections.Generic.ICollection<") ||
			typeName.StartsWith("System.Collections.Generic.HashSet<") ||
			typeName.StartsWith("System.Collections.Generic.Queue<") ||
			typeName.StartsWith("System.Collections.Generic.Stack<") ||
			typeName.EndsWith("[]"))
		{
			string elementType;
			if (typeName.EndsWith("[]"))
			{
				elementType = typeName.Substring(0, typeName.Length - 2);
			}
			else
			{
				var start = typeName.IndexOf('<') + 1;
				var len = typeName.LastIndexOf('>') - start;
				elementType = typeName.Substring(start, len);
			}

			var elementUrn = ToUrn(elementType).Replace("urn:jsonschema:", "");
			return $"urn:jsonschema:array-{elementUrn}";
		}

		// Canonical dictionary shapes share IDs.
		if (typeName.StartsWith("System.Collections.Generic.Dictionary<") ||
			typeName.StartsWith("System.Collections.Generic.IDictionary<") ||
			typeName.StartsWith("System.Collections.Generic.IReadOnlyDictionary<"))
		{
			var start = typeName.IndexOf('<') + 1;
			var len = typeName.LastIndexOf('>') - start;
			var args = typeName.Substring(start, len).Split(',');
			var keyUrn = ToUrn(args[0].Trim()).Replace("urn:jsonschema:", "");
			var valueUrn = ToUrn(args[1].Trim()).Replace("urn:jsonschema:", "");
			return $"urn:jsonschema:object-{keyUrn}-{valueUrn}";
		}

		// Fallback: replace problematic chars for URN
		var sb = new StringBuilder("urn:jsonschema:");
		foreach (var ch in typeName)
		{
			if (char.IsLetterOrDigit(ch)) sb.Append(ch);
			else if (ch == '.') sb.Append('.');
			else if (ch == '<') sb.Append('-');
			else if (ch == '>') continue;
			else if (ch == ',') sb.Append('-');
			else if (ch == '`') sb.Append('G');
			else if (ch == '[') sb.Append("-Arr");
			else if (ch == ']') continue;
			else if (ch == ' ') continue;
			else if (ch == ':') sb.Append(':');
			else sb.Append('-');
		}
		return sb.ToString();
	}

	private static void EmitBuildForTypeMethod(StringBuilder sb, List<TypeInfo> types, List<SchemaHandlerInfo> schemaHandlers, IReadOnlyList<(string TypeName, string SchemaId)> foreignTypeEntries, IReadOnlyList<(string TypeName, string SchemaId)> shapeAliases)
	{
		sb.AppendLine("\tpublic static JsonSchemaBuilder BuildForType(this JsonSchemaBuilder builder, Type type)");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tvar isNullable = Nullable.GetUnderlyingType(type) != null;");
		sb.AppendLine("\t\tvar unwrapped = Nullable.GetUnderlyingType(type) ?? type;");

		foreach (var handler in schemaHandlers)
		{
			if (handler.IsOpenGenericTarget)
			{
				sb.AppendLine($"\t\tif (unwrapped.IsGenericType && unwrapped.GetGenericTypeDefinition() == typeof({handler.TargetTypeName}))");
				sb.AppendLine("\t\t{");
				if (handler.ReturnsBuilder)
					sb.AppendLine($"\t\t\treturn {handler.HandlerTypeName}.Apply(builder, unwrapped);");
				else
				{
					sb.AppendLine($"\t\t\t{handler.HandlerTypeName}.Apply(builder, unwrapped);");
					sb.AppendLine("\t\t\treturn builder;");
				}
				sb.AppendLine("\t\t}");
			}
			else
			{
				sb.AppendLine($"\t\tif (unwrapped == typeof({handler.TargetTypeName}))");
				sb.AppendLine("\t\t{");
				if (handler.ReturnsBuilder)
					sb.AppendLine($"\t\t\treturn {handler.HandlerTypeName}.Apply(builder, unwrapped);");
				else
				{
					sb.AppendLine($"\t\t\t{handler.HandlerTypeName}.Apply(builder, unwrapped);");
					sb.AppendLine("\t\t\treturn builder;");
				}
				sb.AppendLine("\t\t}");
			}
		}

		sb.AppendLine("\t\treturn unwrapped switch");
		sb.AppendLine("\t\t{");

		foreach (var (typeName, schemaId) in shapeAliases)
			sb.AppendLine($"\t\t\tvar t when t == typeof({typeName}) => builder.Ref(\"{schemaId}\"),");

		foreach (var type in types)
		{
			var typeName = type.FullyQualifiedName;
			var schemaId = GetSchemaId(type);
			sb.AppendLine($"\t\t\tvar t when t == typeof({typeName}) => builder.Ref(\"{schemaId}\"),");
		}

		foreach (var (typeName, schemaId) in foreignTypeEntries)
			sb.AppendLine($"\t\t\tvar t when t == typeof({typeName}) => builder.Ref(\"{schemaId}\"),");

		sb.AppendLine("\t\t\tvar t when t == typeof(bool) => isNullable ? builder.Type(SchemaValueType.Boolean, SchemaValueType.Null) : builder.Type(SchemaValueType.Boolean),");
		sb.AppendLine("\t\t\tvar t when t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong) => isNullable ? builder.Type(SchemaValueType.Integer, SchemaValueType.Null) : builder.Type(SchemaValueType.Integer),");
		sb.AppendLine("\t\t\tvar t when t == typeof(float) || t == typeof(double) || t == typeof(decimal) => isNullable ? builder.Type(SchemaValueType.Number, SchemaValueType.Null) : builder.Type(SchemaValueType.Number),");
		sb.AppendLine("\t\t\tvar t when t == typeof(string) => builder.Type(SchemaValueType.String),");
		sb.AppendLine("\t\t\tvar t when t == typeof(DateTime) || t == typeof(DateTimeOffset) => isNullable ? builder.Type(SchemaValueType.String, SchemaValueType.Null).Format(global::Json.Schema.Formats.DateTime) : builder.Type(SchemaValueType.String).Format(global::Json.Schema.Formats.DateTime),");
		sb.AppendLine("\t\t\tvar t when t == typeof(Guid) => isNullable ? builder.Type(SchemaValueType.String, SchemaValueType.Null).Format(global::Json.Schema.Formats.Uuid) : builder.Type(SchemaValueType.String).Format(global::Json.Schema.Formats.Uuid),");
		sb.AppendLine("\t\t\tvar t when t == typeof(Uri) => isNullable ? builder.Type(SchemaValueType.String, SchemaValueType.Null).Format(global::Json.Schema.Formats.Uri) : builder.Type(SchemaValueType.String).Format(global::Json.Schema.Formats.Uri),");
		sb.AppendLine("\t\t\tvar t when t == typeof(System.Text.Json.JsonDocument) || t == typeof(System.Text.Json.JsonElement) || t == typeof(System.Text.Json.Nodes.JsonNode) || t == typeof(System.Text.Json.Nodes.JsonValue) => builder,");
		sb.AppendLine("\t\t\tvar t when t == typeof(System.Text.Json.Nodes.JsonObject) => isNullable ? builder.Type(SchemaValueType.Object, SchemaValueType.Null) : builder.Type(SchemaValueType.Object),");
		sb.AppendLine("\t\t\tvar t when t == typeof(System.Text.Json.Nodes.JsonArray) => isNullable ? builder.Type(SchemaValueType.Array, SchemaValueType.Null) : builder.Type(SchemaValueType.Array),");
		sb.AppendLine("\t\t\t_ => builder");
		sb.AppendLine("\t\t};");
		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static List<(string TypeName, string SchemaId, string CanonicalPropertyName)> GetShapeAliases(List<TypeInfo> types)
	{
		var result = new List<(string TypeName, string SchemaId, string CanonicalPropertyName)>();

		var arrayGroups = types
			.Where(t => t.Kind == TypeKind.Array)
			.Where(t => !string.IsNullOrEmpty(GetArrayShapeKey(t.TypeSymbol)))
			.GroupBy(t => GetArrayShapeKey(t.TypeSymbol));

		foreach (var group in arrayGroups)
		{
			var groupTypes = group.ToList();
			if (groupTypes.Count <= 1) continue;

			var canonical = groupTypes
				.FirstOrDefault(t => IsPreferredCollectionShape(t.TypeSymbol))
				?? groupTypes.OrderBy(t => t.FullyQualifiedName, StringComparer.Ordinal).First();

			var canonicalId = GetSchemaId(canonical);
			var canonicalPropName = GetPropertyName(canonical);

			foreach (var type in groupTypes)
			{
				if (ReferenceEquals(type, canonical)) continue;
				result.Add((type.FullyQualifiedName, canonicalId, canonicalPropName));
			}
		}

		var dictionaryGroups = types
			.Where(t => t.Kind == TypeKind.Dictionary)
			.Where(t => !string.IsNullOrEmpty(GetDictionaryShapeKey(t.TypeSymbol)))
			.GroupBy(t => GetDictionaryShapeKey(t.TypeSymbol));

		foreach (var group in dictionaryGroups)
		{
			var groupTypes = group.ToList();
			if (groupTypes.Count <= 1) continue;

			var canonical = groupTypes
				.FirstOrDefault(t => IsPreferredDictionaryShape(t.TypeSymbol))
				?? groupTypes.OrderBy(t => t.FullyQualifiedName, StringComparer.Ordinal).First();

			var canonicalId = GetSchemaId(canonical);
			var canonicalPropName = GetPropertyName(canonical);

			foreach (var type in groupTypes)
			{
				if (ReferenceEquals(type, canonical)) continue;
				result.Add((type.FullyQualifiedName, canonicalId, canonicalPropName));
			}
		}

		return result;
	}

	private static string GetArrayShapeKey(ITypeSymbol typeSymbol)
	{
		var elementType = CodeEmitterHelpers.GetElementType(typeSymbol);
		if (elementType == null) return string.Empty;

		var unwrappedElement = CodeEmitterHelpers.UnwrapNullable(elementType);
		return unwrappedElement.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}

	private static string GetDictionaryShapeKey(ITypeSymbol typeSymbol)
	{
		var keyType = CodeEmitterHelpers.GetDictionaryKeyType(typeSymbol);
		var valueType = CodeEmitterHelpers.GetDictionaryValueType(typeSymbol);
		if (keyType == null || valueType == null) return string.Empty;

		var unwrappedKey = CodeEmitterHelpers.UnwrapNullable(keyType).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		var unwrappedValue = CodeEmitterHelpers.UnwrapNullable(valueType).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		return $"{unwrappedKey}|{unwrappedValue}";
	}

	private static bool IsPreferredCollectionShape(ITypeSymbol typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedType || !namedType.IsGenericType) return false;

		var typeString = namedType.ConstructedFrom.ToDisplayString();
		return typeString == "System.Collections.Generic.IEnumerable<T>";
	}

	private static bool IsPreferredDictionaryShape(ITypeSymbol typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedType || !namedType.IsGenericType) return false;

		var typeString = namedType.ConstructedFrom
			.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			.Replace(" ", string.Empty);

		return typeString == "global::System.Collections.Generic.IDictionary<TKey,TValue>";
	}

	private static void EmitAliasProperty(StringBuilder sb, TypeInfo type, string canonicalPropertyName)
	{
		sb.AppendLine("\t/// <summary>");
		sb.AppendLine($"\t/// Alias for <see cref=\"{canonicalPropertyName}\"/>, same collection shape.");
		sb.AppendLine("\t/// </summary>");
		sb.AppendLine($"\tpublic static readonly JsonSchema {GetPropertyName(type)} = {canonicalPropertyName};");
		sb.AppendLine();
	}

	private static void EmitRegisterSchemasMethod(StringBuilder sb, List<TypeInfo> types, Dictionary<string, string> aliasPropertyMap)
	{
		sb.AppendLine();
		sb.AppendLine("\t/// <summary>");
		sb.AppendLine("\t/// Registers all generated schemas in the specified schema registry.");
		sb.AppendLine("\t/// </summary>");
		sb.AppendLine("\tpublic static void RegisterSchemas(Json.Schema.SchemaRegistry registry)");
		sb.AppendLine("\t{");

		foreach (var type in types)
		{
			// Skip non-canonical shapes — they share a schema with the canonical type
			if (aliasPropertyMap.ContainsKey(type.FullyQualifiedName)) continue;
			sb.AppendLine($"\t\tregistry.Register({GetPropertyName(type)});");
		}

		sb.AppendLine("\t}");
	}

	private static List<TypeInfo> OrderTypesTopologically(List<TypeInfo> types)
	{
		var graph = new Dictionary<TypeInfo, List<TypeInfo>>();
		var inDegree = new Dictionary<TypeInfo, int>();

		foreach (var type in types)
		{
			graph[type] = new List<TypeInfo>();
			inDegree[type] = 0;
		}

		foreach (var type in types)
		{
			foreach (var prop in type.Properties)
			{
				var depType = FindTypeBySymbol(types, prop.Type);
				if (depType != null && depType != type)
				{
					graph[type].Add(depType);
					inDegree[depType]++;
				}
			}
		}

		var queue = new Queue<TypeInfo>();
		foreach (var type in types)
		{
			if (inDegree[type] == 0)
				queue.Enqueue(type);
		}

		var result = new List<TypeInfo>();
		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			result.Add(current);

			foreach (var dep in graph[current])
			{
				inDegree[dep]--;
				if (inDegree[dep] == 0)
					queue.Enqueue(dep);
			}
		}

		// If there are cycles, just return the original order
		if (result.Count != types.Count)
			return types;

		return result;
	}

	private static TypeInfo? FindTypeBySymbol(List<TypeInfo> types, ITypeSymbol symbol)
	{
		var unwrapped = CodeEmitterHelpers.UnwrapNullable(symbol);

		return types.FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.TypeSymbol, unwrapped));
	}

	private static string GetPropertyName(TypeInfo type) => type.ResolvedPropertyName ?? type.SchemaPropertyName;

	private static void EmitSchemaProperty(StringBuilder sb, TypeInfo type, Dictionary<string, string> typeIds, List<SchemaHandlerInfo> schemaHandlers)
	{
		if (!string.IsNullOrWhiteSpace(type.XmlDocSummary))
		{
			sb.AppendLine("\t/// <summary>");
			sb.AppendLine($"\t/// {CodeEmitterHelpers.EscapeXmlDoc(type.XmlDocSummary!)}");
			sb.AppendLine("\t/// </summary>");
		}
		else
		{
			sb.AppendLine("\t/// <summary>");
			sb.AppendLine($"\t/// Generated schema for {CodeEmitterHelpers.EscapeXmlDoc(type.FullyQualifiedName)}.");
			sb.AppendLine("\t/// </summary>");
		}

		sb.Append($"\tpublic static readonly JsonSchema {GetPropertyName(type)} = ");
		EmitSchemaBuilder(sb, type, 2, typeIds, schemaHandlers);
		sb.AppendLine(".Build();");
		sb.AppendLine();
	}

	private static void EmitSchemaBuilder(StringBuilder sb, TypeInfo type, int indent, Dictionary<string, string> typeIds, List<SchemaHandlerInfo> schemaHandlers)
	{
		var indentStr = new string('\t', indent);
		
		var context = new SchemaEmissionContext(typeIds, schemaHandlers);
		AnalyzeTypeReferences(type, context);
		context.RootType = type.TypeSymbol;
		
		sb.Append("new JsonSchemaBuilder()");

		sb.AppendLine();
		sb.Append($"{indentStr}.Schema(\"https://json-schema.org/draft/2020-12/schema\")");

		var idAttr = type.TypeAttributes.FirstOrDefault(a => a.AttributeName == "IdAttribute");
		string id;
		if (idAttr != null && idAttr.Parameters.TryGetValue("arg0", out var idValue) && idValue is string idStr)
		{
			id = idStr;
		}
		else
		{
			id = ToUrn(type.FullyQualifiedName);
		}
		sb.AppendLine();
		sb.Append($"{indentStr}.Id(\"{id}\")");

		if (context.HasSchemaHandler(type.TypeSymbol))
		{
			sb.AppendLine();
			sb.Append($"{indentStr}.BuildForType(typeof({type.TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}))");
			EmitAttributes(sb, type.TypeAttributes, indentStr);
			return;
		}

		var emitter = SchemaEmitterRegistry.Emitters.FirstOrDefault(e => e.Handles(type));
		if (emitter != null)
		{
			emitter.EmitSchema(sb, type, indentStr, context);
		}
		else
		{
			sb.AppendLine();
			sb.Append($"{indentStr}.Type(SchemaValueType.Object)");
		}

		EmitAttributes(sb, type.TypeAttributes, indentStr);
	}

	internal static void EmitAttributes(StringBuilder sb, List<AttributeInfo> attributes, string indent)
	{
		foreach (var attr in attributes)
		{
			if (attr.Parameters.TryGetValue("ConditionGroup", out var conditionGroup) && conditionGroup != null) continue;

			// Skip IdAttribute as it's handled separately in EmitSchemaBuilder
			if (attr.AttributeName == "IdAttribute") continue;

			if (attr.IsCustomEmitter && attr.AttributeFullName != null)
			{
				sb.AppendLine();
				sb.Append($"{indent}");
				EmitCustomAttributeCall(sb, attr);
				continue;
			}

			if (!ShouldEmitBuiltInAttribute(attr)) continue;

			sb.AppendLine();
			sb.Append($"{indent}");
			EmitAttributeConstraint(sb, attr);
		}
	}

	internal static bool IsBuiltInAttributeNamespace(string? attributeFullName)
	{
		var isJsonSchemaGenerationAttr = attributeFullName?.StartsWith("global::Json.Schema.Generation.") == true;
		var isSpecialSystemAttr = attributeFullName == "global::System.ObsoleteAttribute" ||
		                          attributeFullName == "global::System.Text.Json.Serialization.JsonNumberHandlingAttribute";
		return isJsonSchemaGenerationAttr || isSpecialSystemAttr;
	}

	internal static bool ShouldEmitBuiltInAttribute(AttributeInfo attr)
	{
		return IsBuiltInAttributeNamespace(attr.AttributeFullName);
	}

	internal static void EmitAttributeConstraint(StringBuilder sb, AttributeInfo attr)
	{
		switch (attr.AttributeName)
		{
			case "MinimumAttribute" when attr.Parameters.TryGetValue("arg0", out var minValue):
				sb.Append($".Minimum({CodeEmitterHelpers.FormatValue(minValue)})");
				break;
			case "MaximumAttribute" when attr.Parameters.TryGetValue("arg0", out var maxValue):
				sb.Append($".Maximum({CodeEmitterHelpers.FormatValue(maxValue)})");
				break;
			case "ExclusiveMinimumAttribute" when attr.Parameters.TryGetValue("arg0", out var exMinValue):
				sb.Append($".ExclusiveMinimum({CodeEmitterHelpers.FormatValue(exMinValue)})");
				break;
			case "ExclusiveMaximumAttribute" when attr.Parameters.TryGetValue("arg0", out var exMaxValue):
				sb.Append($".ExclusiveMaximum({CodeEmitterHelpers.FormatValue(exMaxValue)})");
				break;
			case "MinLengthAttribute" when attr.Parameters.TryGetValue("arg0", out var minLen):
				sb.Append($".MinLength({CodeEmitterHelpers.FormatValue(minLen)})");
				break;
			case "MaxLengthAttribute" when attr.Parameters.TryGetValue("arg0", out var maxLen):
				sb.Append($".MaxLength({CodeEmitterHelpers.FormatValue(maxLen)})");
				break;
			case "PatternAttribute" when attr.Parameters.TryGetValue("arg0", out var pattern):
				sb.Append($".Pattern(\"{CodeEmitterHelpers.EscapeString(pattern?.ToString() ?? "")}\")");
				break;
			case "MinItemsAttribute" when attr.Parameters.TryGetValue("arg0", out var minItems):
				sb.Append($".MinItems({CodeEmitterHelpers.FormatValue(minItems)})");
				break;
			case "MaxItemsAttribute" when attr.Parameters.TryGetValue("arg0", out var maxItems):
				sb.Append($".MaxItems({CodeEmitterHelpers.FormatValue(maxItems)})");
				break;
			case "UniqueItemsAttribute":
				sb.Append($".UniqueItems(true)");
				break;
			case "MultipleOfAttribute" when attr.Parameters.TryGetValue("arg0", out var multipleOf):
				sb.Append($".MultipleOf({CodeEmitterHelpers.FormatValue(multipleOf)})");
				break;
			case "TitleAttribute" when attr.Parameters.TryGetValue("arg0", out var title):
				sb.Append($".Title(\"{CodeEmitterHelpers.EscapeString(title?.ToString() ?? "")}\")");
				break;
			case "DescriptionAttribute" when attr.Parameters.TryGetValue("arg0", out var description):
				sb.Append($".Description(\"{CodeEmitterHelpers.EscapeString(description?.ToString() ?? "")}\")");
				break;
			case "ObsoleteAttribute":
				sb.Append(".Deprecated(true)");
				break;
			case "ReadOnlyAttribute":
				sb.Append(".ReadOnly(true)");
				break;
			case "WriteOnlyAttribute":
				sb.Append(".WriteOnly(true)");
				break;
			case "DefaultAttribute" when attr.Parameters.TryGetValue("arg0", out var defaultValue):
				sb.Append($".Default({CodeEmitterHelpers.FormatValue(defaultValue)})");
				break;
			case "AdditionalPropertiesAttribute" when attr.Parameters.TryGetValue("arg0", out var additionalProps) && additionalProps is bool boolVal:
				sb.Append($".AdditionalProperties({(boolVal ? "true" : "false")})");
				break;
		}
	}

	private static void CollectCustomAttributes(List<AttributeInfo> attributes, Dictionary<string, AttributeInfo> customAttributes)
	{
		foreach (var attr in attributes)
		{
			if (attr is { IsCustomEmitter: true, AttributeFullName: not null }) 
				customAttributes.TryAdd(attr.AttributeFullName, attr);
		}
	}

	private static void EmitExtensionMethods(StringBuilder sb, Dictionary<string, AttributeInfo> customAttributes)
	{
		sb.AppendLine("/// <summary>");
		sb.AppendLine("/// Extension methods for custom schema attributes.");
		sb.AppendLine("/// </summary>");
		sb.AppendLine("public static class GeneratedSchemaExtensions");
		sb.AppendLine("{");
		foreach (var kvp in customAttributes)
		{
			var attrFullName = kvp.Key;
			var attrInfo = kvp.Value;
			
			var lastDot = attrFullName.LastIndexOf('.');
			var attrName = lastDot >= 0 ? attrFullName[(lastDot + 1)..] : attrFullName;
			if (attrName.EndsWith("Attribute"))
				attrName = attrName[..^9];

			var paramList = new StringBuilder("this JsonSchemaBuilder builder");
			var argList = new StringBuilder("builder");
			
			if (attrInfo.ApplyMethodParameters != null)
			{
				foreach (var param in attrInfo.ApplyMethodParameters)
				{
					paramList.Append($", {param.TypeName} {param.Name}");
					argList.Append($", {param.Name}");
				}
			}

			sb.AppendLine($"\tpublic static JsonSchemaBuilder {attrName}({paramList})");
			sb.AppendLine("\t{");
			sb.AppendLine($"\t\treturn {attrFullName}.Apply({argList});");
			sb.AppendLine("\t}");
			sb.AppendLine();
		}

		sb.AppendLine("}");
	}

	private static void EmitCustomAttributeCall(StringBuilder sb, AttributeInfo attr)
	{
		var lastDot = attr.AttributeFullName!.LastIndexOf('.');
		var attrName = lastDot >= 0 ? attr.AttributeFullName[(lastDot + 1)..] : attr.AttributeFullName;
		if (attrName.EndsWith("Attribute"))
			attrName = attrName[..^9];

		sb.Append($".{attrName}(");
		
		var first = true;
		
		if (attr.ApplyMethodParameters != null && attr.ApplyMethodParameters.Count > 0)
		{
			foreach (var param in attr.ApplyMethodParameters)
			{
				if (!first)
					sb.Append(", ");
				
				object? value = null;
				var found = false;
				
				for (int i = 0; i < attr.Parameters.Count; i++)
				{
					if (attr.Parameters.TryGetValue($"arg{i}", out value))
					{
						if (i == attr.ApplyMethodParameters.IndexOf(param))
						{
							found = true;
							break;
						}
					}
				}
				
				if (!found)
				{
					foreach (var kvp in attr.Parameters)
					{
						if (!kvp.Key.StartsWith("arg") && 
						    string.Equals(kvp.Key, param.Name, StringComparison.OrdinalIgnoreCase))
						{
							value = kvp.Value;
							found = true;
							break;
						}
					}
				}

				if (found)
					sb.Append(FormatCustomApplyParameterValue(param, value));
				else
					sb.Append("0");

				first = false;
			}
		}
		else
		{
			for (int i = 0; i < attr.Parameters.Count; i++)
			{
				if (attr.Parameters.TryGetValue($"arg{i}", out var value))
				{
					if (!first)
						sb.Append(", ");

					sb.Append(CodeEmitterHelpers.FormatValue(value));
					first = false;
				}
			}
		}
		
		sb.Append(')');
	}

	private static string FormatCustomApplyParameterValue(ApplyParameterInfo parameter, object? value)
	{
		var normalizedTypeName = parameter.TypeName
			.Replace("global::", string.Empty)
			.Replace(" ", string.Empty);

		if (normalizedTypeName is "System.Type" or "Type")
		{
			if (value is ITypeSymbol typeSymbol)
				return $"typeof({typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";

			return $"typeof({value})";
		}

		return CodeEmitterHelpers.FormatValue(value);
	}


	private static void AnalyzeTypeReferences(TypeInfo type, SchemaEmissionContext context)
	{
		foreach (var prop in type.Properties)
		{
			CollectPropertyTypes(prop.Type, context);
		}
	}

	private static void CollectPropertyTypes(ITypeSymbol typeSymbol, SchemaEmissionContext context)
	{
		var unwrapped = CodeEmitterHelpers.UnwrapNullable(typeSymbol);
		if (IsBuiltInJsonDomType(unwrapped)) return;
		
		var typeKind = DetermineTypeKind(unwrapped);
		if (typeKind is TypeKind.Boolean or TypeKind.Integer or TypeKind.Number or 
		    TypeKind.String or TypeKind.DateTime or TypeKind.Guid or TypeKind.Uri or TypeKind.Any)
			return;

		if (typeKind == TypeKind.Enum)
		{
			var typeKey = SchemaEmissionContext.GetTypeKey(unwrapped);
			if (!context.TypeReferences.ContainsKey(typeKey))
			{
				var defName = SchemaEmissionContext.GetDefinitionName(unwrapped);
				context.TypeReferences[typeKey] = (defName, unwrapped);
			}

			return;
		}
		
		if (typeKind == TypeKind.Array)
		{
			var elementType = CodeEmitterHelpers.GetElementType(unwrapped);
			if (elementType != null) 
				CollectPropertyTypes(elementType, context);

			return;
		}

		if (typeKind == TypeKind.Dictionary)
		{
			var keyType = CodeEmitterHelpers.GetDictionaryKeyType(unwrapped);
			if (keyType != null)
				CollectPropertyTypes(keyType, context);

			var valueType = CodeEmitterHelpers.GetDictionaryValueType(unwrapped);
			if (valueType != null)
				CollectPropertyTypes(valueType, context);

			return;
		}
		
		if (typeKind == TypeKind.Object)
		{
			var typeKey = SchemaEmissionContext.GetTypeKey(unwrapped);
			if (!context.TypeReferences.ContainsKey(typeKey))
			{
				var defName = SchemaEmissionContext.GetDefinitionName(unwrapped);
				context.TypeReferences[typeKey] = (defName, unwrapped);
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

	private static void EmitRegistrationClass(StringBuilder sb, List<TypeInfo> types)
	{
		sb.AppendLine("/// <summary>");
		sb.AppendLine("/// Registers generated schemas with the ValidatingJsonConverter for AOT scenarios.");
		sb.AppendLine("/// </summary>");
		sb.AppendLine("[UnconditionalSuppressMessage(\"AOT\", \"IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.\", Justification = \"Suppressing for generated code.\")]");
		sb.AppendLine("internal static class GeneratedSchemaRegistration");
		sb.AppendLine("{");
		sb.AppendLine("\t[ModuleInitializer]");
		sb.AppendLine("\tinternal static void RegisterSchemas()");
		sb.AppendLine("\t{");

		foreach (var type in types)
		{
			sb.AppendLine($"\t\tValidatingJsonConverter.RegisterConverter(");
			sb.AppendLine($"\t\t\ttypeof({type.FullyQualifiedName}),");
			sb.AppendLine($"\t\t\tnew ValidatingJsonConverter<{type.FullyQualifiedName}>(");
				sb.AppendLine($"\t\t\t\tGeneratedJsonSchemas.{GetPropertyName(type)},");
			sb.AppendLine($"\t\t\t\tValidatingJsonConverter.DefaultOptionsFactory));");
		}

		sb.AppendLine("\t}");
		sb.AppendLine("}");
	}
}
