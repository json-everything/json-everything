using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Serialization;
using Json.Schema.Generation.SourceGeneration.Emitters;
using Microsoft.CodeAnalysis;

namespace Json.Schema.Generation.SourceGeneration;

internal static class TypeAnalyzer
{
	public static TypeInfo? Analyze(Compilation compilation, INamedTypeSymbol typeSymbol, AttributeData? attributeData, Action<Diagnostic> reportDiagnostic, NamingConvention defaultPropertyNaming = NamingConvention.AsDeclared, PropertyOrder defaultPropertyOrder = PropertyOrder.AsDeclared)
	{
		if (typeSymbol is { IsGenericType: true, IsUnboundGenericType: false })
		{
			foreach (var typeArg in typeSymbol.TypeArguments)
			{
				if (typeArg.TypeKind == Microsoft.CodeAnalysis.TypeKind.TypeParameter)
				{
					reportDiagnostic(Diagnostic.Create(
						Diagnostics.OpenGenericTypeNotSupported,
						typeSymbol.Locations.FirstOrDefault(),
						typeSymbol.ToDisplayString()));

					return null;
				}
			}
		}

		var propertyNaming = defaultPropertyNaming;
		var propertyOrder = defaultPropertyOrder;
		var strictConditionals = false;

		if (attributeData != null)
		{
			foreach (var namedArg in attributeData.NamedArguments)
			{
				switch (namedArg.Key)
				{
					case "PropertyNaming" when namedArg.Value.Value is int namingValue:
						propertyNaming = (NamingConvention)namingValue;
						break;
					case "PropertyOrder" when namedArg.Value.Value is int orderValue:
						propertyOrder = (PropertyOrder)orderValue;
						break;
					case "StrictConditionals" when namedArg.Value.Value is bool strictValue:
						strictConditionals = strictValue;
						break;
				}
			}
		}

		var typeKind = DetermineTypeKind(typeSymbol);
		var isNullable = IsNullableType(typeSymbol);

		var typeInfo = new TypeInfo
		{
			TypeSymbol = typeSymbol,
			FullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
			SchemaPropertyName = GetSchemaPropertyName(typeSymbol),
			PropertyNaming = propertyNaming,
			PropertyOrder = propertyOrder,
			StrictConditionals = strictConditionals,
			Kind = typeKind,
			IsNullable = isNullable,
			XmlDocSummary = GetXmlDocSummary(typeSymbol)
		};

		switch (typeKind)
		{
			case TypeKind.Object:
				AnalyzeObjectType(compilation, typeInfo, reportDiagnostic);
				break;
			case TypeKind.Enum:
				AnalyzeEnumType(typeInfo);
				break;
			case TypeKind.Array:
				break;
		}

		ExtractAttributes(compilation, typeSymbol.GetAttributes(), typeInfo.TypeAttributes);

		if (typeKind == TypeKind.Object)
			AnalyzeConditionals(typeInfo);

		return typeInfo;
	}

	private static TypeKind DetermineTypeKind(ITypeSymbol typeSymbol)
	{
		var unwrappedType = UnwrapNullable(typeSymbol);
		var typeString = unwrappedType.ToDisplayString();

		switch (typeString)
		{
			case "System.Text.Json.JsonDocument" or "System.Text.Json.JsonElement" or "System.Text.Json.Nodes.JsonNode" or "System.Text.Json.Nodes.JsonValue":
				return TypeKind.Any;
			case "System.Text.Json.Nodes.JsonObject":
				return TypeKind.Object;
			case "System.Text.Json.Nodes.JsonArray":
				return TypeKind.Array;
			case "bool" or "System.Boolean":
				return TypeKind.Boolean;
			case "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or
				"System.Byte" or "System.SByte" or "System.Int16" or "System.UInt16" or
				"System.Int32" or "System.UInt32" or "System.Int64" or "System.UInt64":
				return TypeKind.Integer;
			case "float" or "double" or "decimal" or "System.Single" or "System.Double" or "System.Decimal":
				return TypeKind.Number;
			case "string" or "System.String":
				return TypeKind.String;
			case "System.DateTime" or "System.DateTimeOffset":
				return TypeKind.DateTime;
			case "System.Guid":
				return TypeKind.Guid;
			case "System.Uri":
				return TypeKind.Uri;
		}

		if (unwrappedType.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum) return TypeKind.Enum;
		if (unwrappedType is IArrayTypeSymbol) return TypeKind.Array;
		if (CodeEmitterHelpers.IsCollectionType(unwrappedType)) return TypeKind.Array;
		if (CodeEmitterHelpers.IsDictionaryType(unwrappedType)) return TypeKind.Dictionary;

		return TypeKind.Object;
	}

	private static void AnalyzeObjectType(Compilation compilation, TypeInfo typeInfo, Action<Diagnostic> reportDiagnostic)
	{
		var typeSymbol = (INamedTypeSymbol)typeInfo.TypeSymbol;
		var encounteredSchemaNames = new HashSet<string>(StringComparer.Ordinal);

		var members = typeSymbol.GetMembers()
			.Where(m => m is { IsStatic: false, Kind: SymbolKind.Property or SymbolKind.Field })
			.ToList();

		foreach (var member in members)
		{
			if (HasAttribute(member, "JsonExcludeAttribute")) continue;

			var jsonIgnoreAttr = member.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass?.Name == "JsonIgnoreAttribute");

			if (jsonIgnoreAttr != null)
			{
				var conditionArg = jsonIgnoreAttr.NamedArguments
					.FirstOrDefault(arg => arg.Key == "Condition");

				if (conditionArg.Value.Value == null || (int)conditionArg.Value.Value == 1) continue;
			}

			ITypeSymbol memberType;
			bool isReadOnly;
			var isWriteOnly = false;

			if (member is IPropertySymbol property)
			{
				if (property.DeclaredAccessibility != Accessibility.Public && !HasAttribute(member, "JsonIncludeAttribute")) continue;

				if (property.IsIndexer) continue;

				memberType = property.Type;
				isReadOnly = property.SetMethod is not { DeclaredAccessibility: Accessibility.Public };
				isWriteOnly = property.GetMethod is not { DeclaredAccessibility: Accessibility.Public };

				if (isWriteOnly) continue;
			}
			else if (member is IFieldSymbol field)
			{
				if (field.DeclaredAccessibility != Accessibility.Public && !HasAttribute(member, "JsonIncludeAttribute")) continue;

				memberType = field.Type;
				isReadOnly = field.IsReadOnly;
			}
			else continue;

			var schemaName = GetPropertySchemaName(member, typeInfo.PropertyNaming);
			if (!encounteredSchemaNames.Add(schemaName))
			{
				reportDiagnostic(Diagnostic.Create(
					Diagnostics.DuplicateSchemaPropertyName,
					member.Locations.FirstOrDefault(),
					typeSymbol.ToDisplayString(),
					schemaName));
				continue;
			}

			var isRequired = false;
			var requiredAttrs = member.GetAttributes().Where(a => a.AttributeClass?.Name == "RequiredAttribute").ToList();
			foreach (var reqAttr in requiredAttrs)
			{
				var hasConditionGroup = false;
				foreach (var namedArg in reqAttr.NamedArguments)
				{
					if (namedArg is { Key: "ConditionGroup", Value.Value: not null })
					{
						hasConditionGroup = true;
						break;
					}
				}

				if (!hasConditionGroup)
				{
					isRequired = true;
					break;
				}
			}

			if (!isRequired)
				isRequired = HasAttribute(member, "JsonRequiredAttribute");

			if (!isRequired && member is IPropertySymbol propertySymbol)
				isRequired = propertySymbol.IsRequired;

			var memberAttributes = member.GetAttributes();
			var isNullable = GetNullableAttributeOverride(memberAttributes) ?? IsNullableType(memberType);

			var propertyInfo = new PropertyInfo
			{
				Name = member.Name,
				SchemaName = schemaName,
				Type = memberType,
				IsRequired = isRequired,
				IsNullable = isNullable,
				IsReadOnly = isReadOnly,
				IsWriteOnly = isWriteOnly,
				XmlDocSummary = GetXmlDocSummary(member)
			};

			ExtractAttributes(compilation, memberAttributes, propertyInfo.Attributes);

			typeInfo.Properties.Add(propertyInfo);
		}

		if (typeInfo.PropertyOrder == PropertyOrder.ByName)
			typeInfo.Properties.Sort((a, b) => string.Compare(a.SchemaName, b.SchemaName, StringComparison.OrdinalIgnoreCase));
	}

	private static void AnalyzeEnumType(TypeInfo typeInfo)
	{
		var typeSymbol = typeInfo.TypeSymbol;

		foreach (var member in typeSymbol.GetMembers())
		{
			if (member is IFieldSymbol { IsConst: true, HasConstantValue: true } field)
			{
				if (CodeEmitterHelpers.ShouldIncludeEnumMember(field))
					typeInfo.EnumValues.Add(field.Name);
			}
		}
	}

	private static void AnalyzeConditionals(TypeInfo typeInfo)
	{
		var conditionalsByGroup = new Dictionary<object, ConditionalInfo>();

		var typeAttributes = typeInfo.TypeSymbol.GetAttributes();

		foreach (var attr in typeAttributes)
		{
			if (attr.AttributeClass == null) continue;

			var attrName = attr.AttributeClass.Name;
			object? conditionGroup = null;

			foreach (var namedArg in attr.NamedArguments)
			{
				if (namedArg.Key == "ConditionGroup")
				{
					conditionGroup = namedArg.Value.Value;
					break;
				}
			}

			if (conditionGroup == null && attr.ConstructorArguments.Length > 0)
			{
				var lastArg = attr.ConstructorArguments[^1];
				if (lastArg.Value != null)
					conditionGroup = lastArg.Value;
			}

			if (conditionGroup == null) continue;

			if (!conditionalsByGroup.TryGetValue(conditionGroup, out var conditionalInfo))
			{
				conditionalInfo = new ConditionalInfo { ConditionGroup = conditionGroup };
				conditionalsByGroup[conditionGroup] = conditionalInfo;
			}

			switch (attrName)
			{
				case "IfAttribute":
					ParseIfAttribute(attr, conditionalInfo, typeInfo);
					break;
				case "IfMinAttribute":
					ParseIfMinAttribute(attr, conditionalInfo, typeInfo);
					break;
				case "IfMaxAttribute":
					ParseIfMaxAttribute(attr, conditionalInfo, typeInfo);
					break;
				case "IfEnumAttribute":
					ParseIfEnumAttribute(attr, typeInfo, conditionalsByGroup);
					break;
			}
		}

		foreach (var property in typeInfo.Properties)
		{
			foreach (var attr in property.Attributes)
			{
				if (!attr.Parameters.TryGetValue("ConditionGroup", out var conditionGroupValue) || conditionGroupValue == null)
					continue;

				var conditionGroup = conditionGroupValue;

				if (!conditionalsByGroup.TryGetValue(conditionGroup, out var conditionalInfo))
				{
					conditionalInfo = new ConditionalInfo { ConditionGroup = conditionGroup };
					conditionalsByGroup[conditionGroup] = conditionalInfo;
				}

				if (!property.ConditionGroups.Contains(conditionGroup))
					property.ConditionGroups.Add(conditionGroup);

				var existingConsequence = conditionalInfo.PropertyConsequences
					.FirstOrDefault(c => c.PropertySchemaName == property.SchemaName);

				var isRequired = existingConsequence?.IsConditionallyRequired ?? false;
				var isReadOnly = existingConsequence?.IsConditionallyReadOnly ?? false;
				var isWriteOnly = existingConsequence?.IsConditionallyWriteOnly ?? false;
				var conditionalAttributes = existingConsequence?.ConditionalAttributes ?? new List<AttributeInfo>();
				// Set boolean flags for special cases
				switch (attr.AttributeName)
				{
					case "RequiredAttribute":
						isRequired = true;
						break;
					case "ReadOnlyAttribute":
						if (attr.Parameters.TryGetValue("arg0", out var readOnlyValue) && readOnlyValue is not false)
							isReadOnly = true;
						break;
					case "WriteOnlyAttribute":
						if (attr.Parameters.TryGetValue("arg0", out var writeOnlyValue) && writeOnlyValue is not false)
							isWriteOnly = true;
						break;
				}

				// Add all validation attributes to the list (except RequiredAttribute which is handled separately)
				if (attr.AttributeName != "RequiredAttribute" && SchemaCodeEmitter.ShouldEmitBuiltInAttribute(attr))
					conditionalAttributes.Add(attr);

				var newConsequence = new PropertyConditionalConsequence
				{
					PropertySchemaName = property.SchemaName,
					IsConditionallyRequired = isRequired,
					IsConditionallyReadOnly = isReadOnly,
					IsConditionallyWriteOnly = isWriteOnly,
					ConditionalAttributes = conditionalAttributes
				};

				if (existingConsequence != null)
					conditionalInfo.PropertyConsequences.Remove(existingConsequence);

				conditionalInfo.PropertyConsequences.Add(newConsequence);
			}
		}

		typeInfo.Conditionals.AddRange(conditionalsByGroup.Values);
	}

	private static void ParseIfAttribute(AttributeData attr, ConditionalInfo conditionalInfo, TypeInfo typeInfo)
	{
		if (attr.ConstructorArguments.Length < 2) return;

		var propertyName = attr.ConstructorArguments[0].Value?.ToString();
		var value = attr.ConstructorArguments[1].Value;

		if (string.IsNullOrEmpty(propertyName)) return;

		var property = typeInfo.Properties.FirstOrDefault(p => p.Name == propertyName);
		var schemaName = property?.SchemaName ?? ApplyNamingConvention(propertyName!, typeInfo.PropertyNaming);

		var trigger = new ConditionalTrigger
		{
			Type = ConditionalTriggerType.Equality,
			PropertyName = propertyName!,
			PropertySchemaName = schemaName,
			ExpectedValue = FormatValueForSchema(value)
		};

		conditionalInfo.Triggers.Add(trigger);
	}

	private static void ParseIfMinAttribute(AttributeData attr, ConditionalInfo conditionalInfo, TypeInfo typeInfo) =>
		ParseIfNumericBoundAttribute(attr, conditionalInfo, typeInfo, ConditionalTriggerType.Minimum);

	private static void ParseIfMaxAttribute(AttributeData attr, ConditionalInfo conditionalInfo, TypeInfo typeInfo) =>
		ParseIfNumericBoundAttribute(attr, conditionalInfo, typeInfo, ConditionalTriggerType.Maximum);

	private static void ParseIfNumericBoundAttribute(AttributeData attr, ConditionalInfo conditionalInfo, TypeInfo typeInfo, ConditionalTriggerType triggerType)
	{
		if (attr.ConstructorArguments.Length < 2) return;

		var propertyName = attr.ConstructorArguments[0].Value?.ToString();
		var value = attr.ConstructorArguments[1].Value;

		if (string.IsNullOrEmpty(propertyName)) return;

		var property = typeInfo.Properties.FirstOrDefault(p => p.Name == propertyName);
		var schemaName = property?.SchemaName ?? ApplyNamingConvention(propertyName!, typeInfo.PropertyNaming);

		var isExclusive = false;
		foreach (var namedArg in attr.NamedArguments)
		{
			if (namedArg is { Key: "IsExclusive", Value.Value: bool exclusive })
			{
				isExclusive = exclusive;
				break;
			}
		}

		var trigger = new ConditionalTrigger
		{
			Type = triggerType,
			PropertyName = propertyName!,
			PropertySchemaName = schemaName,
			NumericValue = Convert.ToDouble(value),
			IsExclusive = isExclusive
		};

		conditionalInfo.Triggers.Add(trigger);
	}

	private static void ParseIfEnumAttribute(AttributeData attr, TypeInfo typeInfo, Dictionary<object, ConditionalInfo> conditionalsByGroup)
	{
		if (attr.ConstructorArguments.Length < 1) return;

		var propertyName = attr.ConstructorArguments[0].Value?.ToString();
		if (string.IsNullOrEmpty(propertyName)) return;

		var property = typeInfo.Properties.FirstOrDefault(p => p.Name == propertyName);
		if (property == null) return;

		var schemaName = property.SchemaName;

		var propertyType = UnwrapNullable(property.Type);
		if (propertyType.TypeKind != Microsoft.CodeAnalysis.TypeKind.Enum) return;

		var useNumbers = false;
		if (attr.ConstructorArguments.Length > 1 && attr.ConstructorArguments[1].Value is bool un)
			useNumbers = un;

		var enumMembers = propertyType.GetMembers()
			.OfType<IFieldSymbol>()
			.Where(f => f is { IsConst: true, HasConstantValue: true })
			.ToList();

		foreach (var enumMember in enumMembers)
		{
			var enumValue = enumMember.ConstantValue;
			if (enumValue == null) continue;

			var conditionGroup = enumValue;

			if (!conditionalsByGroup.TryGetValue(conditionGroup, out var conditionalInfo))
			{
				conditionalInfo = new ConditionalInfo { ConditionGroup = conditionGroup };
				conditionalsByGroup[conditionGroup] = conditionalInfo;
			}

			var trigger = new ConditionalTrigger
			{
				Type = ConditionalTriggerType.Equality,
				PropertyName = propertyName!,
				PropertySchemaName = schemaName,
				ExpectedValue = useNumbers ? enumValue.ToString() : $"\"{enumMember.Name}\""
			};

			conditionalInfo.Triggers.Add(trigger);
		}
	}

	private static string FormatValueForSchema(object? value)
	{
		return value switch
		{
			null => "null",
			bool b => b ? "true" : "false",
			string s => $"\"{s}\"",
			_ => value.ToString() ?? "null"
		};
	}

	private static void ExtractAttributes(Compilation compilation, IEnumerable<AttributeData> attributes, List<AttributeInfo> targetList)
	{
		foreach (var attr in attributes)
		{
			var attrClass = attr.AttributeClass;
			if (attrClass == null) continue;

			var attrName = attrClass.Name;
			var attrFullName = attrClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			var isCustomEmitter = ImplementsInterface(attrClass, "IAttributeHandler") && HasStaticApplyMethod(attrClass);

			INamedTypeSymbol? handlerClass = null;
			if (!isCustomEmitter)
			{
				handlerClass = FindAttributeHandler(compilation, attrClass);
				if (handlerClass != null)
					isCustomEmitter = true;
			}

			if (!isCustomEmitter && !SchemaCodeEmitter.IsBuiltInAttributeNamespace(attrFullName)) continue;

			List<ApplyParameterInfo>? applyParams = null;
			if (isCustomEmitter)
			{
				var emitterClass = handlerClass ?? attrClass;
				applyParams = ExtractApplyMethodParameters(emitterClass);
			}

			var attrInfo = new AttributeInfo
			{
				AttributeName = attrName,
				IsCustomEmitter = isCustomEmitter,
				AttributeFullName = isCustomEmitter ? (handlerClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? attrFullName) : attrFullName,
				ApplyMethodParameters = applyParams
			};

			for (int i = 0; i < attr.ConstructorArguments.Length; i++)
			{
				var arg = attr.ConstructorArguments[i];
				attrInfo.Parameters[$"arg{i}"] = arg.Value;
			}

			foreach (var namedArg in attr.NamedArguments)
			{
				attrInfo.Parameters[namedArg.Key] = namedArg.Value.Value;
			}

			targetList.Add(attrInfo);
		}
	}

	private static List<ApplyParameterInfo>? ExtractApplyMethodParameters(INamedTypeSymbol attrClass)
	{
		var applyMethod = attrClass.GetMembers("Apply")
			.OfType<IMethodSymbol>()
			.FirstOrDefault(m => m is { IsStatic: true, Name: "Apply" });

		if (applyMethod == null) return null;

		var parameters = new List<ApplyParameterInfo>();
		foreach (var param in applyMethod.Parameters.Skip(1))
		{
			parameters.Add(new ApplyParameterInfo
			{
				Name = param.Name,
				TypeName = param.Type.ToDisplayString()
			});
		}

		return parameters;
	}

	private static bool ImplementsInterface(INamedTypeSymbol typeSymbol, string interfaceName) =>
		typeSymbol.AllInterfaces.Any(i => i.Name == interfaceName);

	private static bool HasStaticApplyMethod(INamedTypeSymbol typeSymbol) =>
		typeSymbol.GetMembers("Apply")
			.OfType<IMethodSymbol>()
			.Any(m => m.IsStatic && m.Name == "Apply" &&
			          m.Parameters.Length > 0 &&
			          m.Parameters[0].Type.Name == "JsonSchemaBuilder");

	private static INamedTypeSymbol? FindAttributeHandler(Compilation compilation, INamedTypeSymbol attributeClass)
	{
		var handlerType = FindHandlerByInterface(compilation.Assembly.GlobalNamespace, attributeClass);
		if (handlerType != null && HasStaticApplyMethod(handlerType))
			return handlerType;

		foreach (var reference in compilation.References)
		{
			var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
			if (assemblySymbol == null) continue;

			handlerType = FindHandlerByInterface(assemblySymbol.GlobalNamespace, attributeClass);
			if (handlerType != null && HasStaticApplyMethod(handlerType))
				return handlerType;
		}

		return null;
	}

	private static INamedTypeSymbol? FindHandlerByInterface(INamespaceSymbol namespaceSymbol, INamedTypeSymbol attributeClass)
	{
		foreach (var type in namespaceSymbol.GetTypeMembers())
		{
			if (type.TypeKind != Microsoft.CodeAnalysis.TypeKind.Class) continue;

			foreach (var iface in type.AllInterfaces)
			{
				if (iface.Name != "IAttributeHandler") continue;
				if (!iface.IsGenericType || iface.TypeArguments.Length != 1) continue;

				var typeArg = iface.TypeArguments[0];
				if (SymbolEqualityComparer.Default.Equals(typeArg, attributeClass))
					return type;
			}
		}

		foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
		{
			var foundType = FindHandlerByInterface(nestedNamespace, attributeClass);
			if (foundType != null)
				return foundType;
		}

		return null;
	}

	private static string GetPropertySchemaName(ISymbol member, NamingConvention naming)
	{
		var jsonPropertyNameAttr = member.GetAttributes()
			.FirstOrDefault(a => a.AttributeClass?.Name == "JsonPropertyNameAttribute");

		return jsonPropertyNameAttr is { ConstructorArguments.Length: > 0 }
			? jsonPropertyNameAttr.ConstructorArguments[0].Value?.ToString() ?? member.Name
			: ApplyNamingConvention(member.Name, naming);
	}

	private static string ApplyNamingConvention(string name, NamingConvention naming) =>
		naming switch
		{
			NamingConvention.CamelCase => ToCamelCase(name),
			NamingConvention.PascalCase => ToPascalCase(name),
			NamingConvention.LowerSnakeCase => ToSnakeCase(name).ToLowerInvariant(),
			NamingConvention.UpperSnakeCase => ToSnakeCase(name).ToUpperInvariant(),
			NamingConvention.KebabCase => ToKebabCase(name),
			NamingConvention.UpperKebabCase => ToKebabCase(name).ToUpperInvariant(),
			_ => name
		};

	private static string ToCamelCase(string name) =>
		string.IsNullOrEmpty(name) || char.IsLower(name[0])
			? name
			: char.ToLowerInvariant(name[0]) + name[1..];

	private static string ToPascalCase(string name) =>
		string.IsNullOrEmpty(name) || char.IsUpper(name[0])
			? name
			: char.ToUpperInvariant(name[0]) + name[1..];

	private static string ToSnakeCase(string name)
	{
		if (string.IsNullOrEmpty(name)) return name;

		var result = new System.Text.StringBuilder();
		result.Append(char.ToLowerInvariant(name[0]));

		for (int i = 1; i < name.Length; i++)
		{
			if (char.IsUpper(name[i]))
			{
				result.Append('_');
				result.Append(char.ToLowerInvariant(name[i]));
			}
			else
				result.Append(name[i]);
		}

		return result.ToString();
	}

	private static string ToKebabCase(string name) => ToSnakeCase(name).Replace('_', '-');

	private static string GetSchemaPropertyName(INamedTypeSymbol typeSymbol)
	{
		var currentName = typeSymbol.Name;

		if (typeSymbol.IsGenericType && typeSymbol.TypeArguments.Length > 0)
		{
			var typeArgNames = typeSymbol.TypeArguments
				.Select(GetTypeArgumentPropertyName);
			currentName = $"{currentName}Of{string.Join("And", typeArgNames)}";
		}

		return typeSymbol.ContainingType != null
			? $"{GetSchemaPropertyName(typeSymbol.ContainingType)}_{currentName}"
			: currentName;
	}

	private static string GetTypeArgumentPropertyName(ITypeSymbol typeSymbol)
	{
		var isNullable = typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
		                 typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
		var unwrapped = UnwrapNullable(typeSymbol);

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
		var sb = new System.Text.StringBuilder(name.Length);
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

	/// Returns the value of [Nullable(bool)] if the attribute is present; null otherwise.
	/// Only recognises Json.Schema.Generation.NullableAttribute to avoid picking up the
	/// compiler-generated System.Runtime.CompilerServices.NullableAttribute.
	private static bool? GetNullableAttributeOverride(IEnumerable<AttributeData> attributes)
	{
		var attr = attributes.FirstOrDefault(a =>
			a.AttributeClass?.Name == "NullableAttribute" &&
			a.AttributeClass.ContainingNamespace?.ToDisplayString() == "Json.Schema.Generation");

		if (attr == null) return null;

		// [Nullable] with no argument defaults to true
		if (attr.ConstructorArguments.Length == 0) return true;

		return attr.ConstructorArguments[0].Value is bool value ? value : true;
	}

	private static bool IsNullableType(ITypeSymbol typeSymbol) =>
		typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
		typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

	private static ITypeSymbol UnwrapNullable(ITypeSymbol typeSymbol) =>
		typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
		typeSymbol is INamedTypeSymbol namedType
			? namedType.TypeArguments[0]
			: typeSymbol;

	private static bool HasAttribute(ISymbol symbol, string attributeName) =>
		symbol.GetAttributes().Any(a => a.AttributeClass?.Name == attributeName);

	private static string? GetXmlDocSummary(ISymbol symbol)
	{
		var xml = symbol.GetDocumentationCommentXml();
		if (string.IsNullOrWhiteSpace(xml)) return null;

		var summaryStart = xml!.IndexOf("<summary>", StringComparison.Ordinal);
		var summaryEnd = xml.IndexOf("</summary>", StringComparison.Ordinal);

		if (summaryStart >= 0 && summaryEnd > summaryStart)
		{
			var summaryText = xml.Substring(summaryStart + 9, summaryEnd - summaryStart - 9);

			// Normalise multi-line summaries: trim each line, discard blank lines,
			// then join into a single space-separated string.
			var lines = summaryText
				.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
				.Select(l => l.Trim())
				.Where(l => l.Length > 0);

			var joined = string.Join(" ", lines);
			return joined.Length > 0 ? joined : null;
		}

		return null;
	}
}
