using System.Linq;
using Microsoft.CodeAnalysis;

namespace Json.Schema.Generation.SourceGeneration.Emitters;

/// <summary>
/// Helper methods for code emission.
/// </summary>
internal static class CodeEmitterHelpers
{
	public static string FormatValue(object? value)
	{
		return value switch
		{
			null => "null",
			string s => $"\"{EscapeString(s)}\"",
			bool b => b ? "true" : "false",
			decimal d => d.ToString(System.Globalization.CultureInfo.InvariantCulture) + "m",
			double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture) + "m",
			float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture) + "m",
			_ => value.ToString() ?? "null"
		};
	}

	public static string EscapeString(string str)
	{
		return str
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("\n", "\\n")
			.Replace("\r", "\\r")
			.Replace("\t", "\\t");
	}

	public static string EscapeXmlDoc(string str)
	{
		return str
			.Replace("&", "&amp;")
			.Replace("<", "&lt;")
			.Replace(">", "&gt;");
	}

	public static ITypeSymbol UnwrapNullable(ITypeSymbol typeSymbol)
	{
		if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
		    typeSymbol is INamedTypeSymbol namedType)
		{
			return namedType.TypeArguments[0];
		}
		return typeSymbol;
	}

	public static bool IsCollectionType(ITypeSymbol typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedType) return false;
		if (namedType.SpecialType == SpecialType.System_String) return false;
		if (IsDictionaryType(namedType)) return false;

		if (TryGetEnumerableElementType(namedType, out _))
			return true;

		return false;
	}

	public static bool IsDictionaryType(ITypeSymbol typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol { IsGenericType: true } namedType) return false;

		var typeString = namedType.ConstructedFrom
			.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			.Replace(" ", string.Empty);

		return typeString is
			"global::System.Collections.Generic.IDictionary<TKey,TValue>" or
			"global::System.Collections.Generic.Dictionary<TKey,TValue>" or
			"global::System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>";
	}

	public static bool IsStringKeyDictionaryType(ITypeSymbol typeSymbol)
	{
		if (!IsDictionaryType(typeSymbol)) return false;
		if (typeSymbol is not INamedTypeSymbol namedType) return false;

		return namedType.TypeArguments[0].SpecialType == SpecialType.System_String;
	}

	public static bool IsEnumKeyDictionaryType(ITypeSymbol typeSymbol)
	{
		if (!IsDictionaryType(typeSymbol)) return false;
		if (typeSymbol is not INamedTypeSymbol namedType) return false;

		return namedType.TypeArguments[0].TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum;
	}

	public static ITypeSymbol? GetDictionaryValueType(ITypeSymbol typeSymbol)
	{
		if (!IsDictionaryType(typeSymbol)) return null;
		if (typeSymbol is not INamedTypeSymbol namedType) return null;

		return namedType.TypeArguments[1];
	}

	public static ITypeSymbol? GetDictionaryKeyType(ITypeSymbol typeSymbol)
	{
		if (!IsDictionaryType(typeSymbol)) return null;
		if (typeSymbol is not INamedTypeSymbol namedType) return null;

		return namedType.TypeArguments[0];
	}

	public static ITypeSymbol? GetElementType(ITypeSymbol typeSymbol)
	{
		var fullyQualifiedTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		if (fullyQualifiedTypeName == "global::System.Text.Json.Nodes.JsonArray")
			return null;

		switch (typeSymbol)
		{
			case IArrayTypeSymbol arrayType:
				return arrayType.ElementType;
			case INamedTypeSymbol namedType:
			{
				if (namedType.SpecialType == SpecialType.System_String) break;
				if (IsDictionaryType(namedType)) break;

				if (TryGetEnumerableElementType(namedType, out var elementType))
					return elementType;

				break;
			}
		}

		return null;
	}

	private static bool TryGetEnumerableElementType(INamedTypeSymbol typeSymbol, out ITypeSymbol? elementType)
	{
		elementType = null;

		if (typeSymbol is { IsGenericType: true } && IsGenericIEnumerable(typeSymbol.ConstructedFrom))
		{
			elementType = typeSymbol.TypeArguments[0];
			return true;
		}

		foreach (var iface in typeSymbol.AllInterfaces)
		{
			if (!iface.IsGenericType) continue;
			if (!IsGenericIEnumerable(iface.ConstructedFrom)) continue;

			elementType = iface.TypeArguments[0];
			return true;
		}

		return false;
	}

	private static bool IsGenericIEnumerable(INamedTypeSymbol typeSymbol)
	{
		return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace(" ", string.Empty)
			== "global::System.Collections.Generic.IEnumerable<T>";
	}

	public static bool ShouldIncludeEnumMember(IFieldSymbol field)
	{
		if (field.GetAttributes().Any(a => a.AttributeClass?.Name == "JsonExcludeAttribute"))
			return false;
		
		var jsonIgnoreAttr = field.GetAttributes()
			.FirstOrDefault(a => a.AttributeClass?.Name == "JsonIgnoreAttribute");
		
		if (jsonIgnoreAttr != null)
		{
			var conditionArg = jsonIgnoreAttr.NamedArguments
				.FirstOrDefault(arg => arg.Key == "Condition");
			
			if (conditionArg.Value.Value == null || (int)conditionArg.Value.Value == 1)
				return false;
		}

		return true;
	}
}
