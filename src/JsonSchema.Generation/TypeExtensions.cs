using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Humanizer;
using Json.More;
#pragma warning disable IL2075

namespace Json.Schema.Generation;

/// <summary>
/// Provides informative methods for types.
/// </summary>
public static class TypeExtensions
{
	/// <summary>
	/// Determines whether the type is a simple, one-dimensional, non-keyed collection.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <returns>true if the type represents an array; false otherwise.</returns>
	public static bool IsArray(this Type type)
	{
		return type != typeof(string) &&
		       (type.IsArray ||
		        type == typeof(Array) ||
		        typeof(IEnumerable).IsAssignableFrom(type));
	}

	internal static Type GetMemberType(this MemberInfo info) =>
		info switch
		{
			FieldInfo fieldInfo => fieldInfo.FieldType,
			PropertyInfo propertyInfo => propertyInfo.PropertyType,
			_ => throw new NotSupportedException($"Cannot get type of {info.GetType()}")
		};

	internal static bool IsReadOnly(this MemberInfo info) =>
		info switch
		{
			FieldInfo fieldInfo => fieldInfo.IsInitOnly,
			PropertyInfo propertyInfo => !propertyInfo.CanWrite,
			_ => throw new NotSupportedException($"Cannot get readability of {info.GetType()}")
		};

	internal static bool IsWriteOnly(this MemberInfo info) =>
		info switch
		{
			FieldInfo => false,
			PropertyInfo propertyInfo => !propertyInfo.CanRead,
			_ => throw new NotSupportedException($"Cannot get writability of {info.GetType()}")
		};

	// ReSharper disable once IdentifierTypo
	private static readonly Dictionary<Type, string> _keywordedTypes =
		new()
		{
			[typeof(void)] = "void",
			[typeof(string)] = "string",
			[typeof(int)] = "int",
			[typeof(short)] = "short",
			[typeof(long)] = "long",
			[typeof(ulong)] = "ulong",
			[typeof(uint)] = "uint",
			[typeof(ushort)] = "ushort",
			[typeof(double)] = "double",
			[typeof(float)] = "float",
			[typeof(byte)] = "byte",
			[typeof(char)] = "char",
			[typeof(bool)] = "bool",
		};

	internal static bool IsJsonType(this Type type) =>
		_keywordedTypes.ContainsKey(type) ||
		type.IsSubclassOf(typeof(JsonNode));

	internal static string CSharpName(this Type type, StringBuilder? sb = null)
	{
		if (_keywordedTypes.TryGetValue(type, out var keyword)) return keyword;

		sb ??= new StringBuilder();
		var name = type.Name;
		if (!type.IsGenericType)
		{
			if (type is { IsNested: true, IsGenericParameter: false })
				name = $"{type.DeclaringType!.CSharpName()}.{name}";
			return name;
		}

		if (type.GetGenericTypeDefinition() == typeof(Nullable<>)) return $"{CSharpName(type.GetGenericArguments()[0])}?";

		sb.Append(name[..name.IndexOf('`')]);
		sb.Append('<');
		sb.Append(string.Join(", ", type.GetGenericArguments()
			.Select(x => CSharpName(x, sb))));
		sb.Append('>');
		name = sb.ToString();

		if (type.IsNested)
			name = $"{type.DeclaringType!.CSharpName()}.{name}";

		return name;
	}

	/// <summary>
	/// Determines if the type is a nullable value type, i.e. <see cref="Nullable{T}"/>.
	/// </summary>
	/// <param name="type">The type</param>
	/// <returns>True if the type is <see cref="Nullable{T}"/>; false otherwise.</returns>
	public static bool IsNullableValueType(this Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
	}

	/// <summary>
	/// Determines if the type is a nullable value type, i.e. <see cref="Nullable{T}"/>.
	/// </summary>
	/// <param name="type">The type</param>
	/// <returns>True if the type is <see cref="Nullable{T}"/>; false otherwise.</returns>
	public static bool IsNullableNumber(this Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
			   type.GetGenericArguments()[0].IsNumber();
	}

	internal static string GetDefName(this Type type) =>
		SchemaGenerationContextOptimizer.TypeNameGenerator?.GenerateName(type) ??
		GetName(type).Camelize();

	private static string GetName(Type type)
	{
		if (type.IsInteger()) return "integer";
		if (type.IsNumber()) return "number";
		if (type == typeof(string)) return "string";
		if (type == typeof(bool)) return "boolean";
		if (type.IsArray)
		{
			var itemType = type.GetElementType();
			return $"array of {GetName(itemType!)}";
		}
		if (type.IsGenericType &&
		    typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()) &&
		    type.GenericTypeArguments.Length == 1)
		{
			var itemType = type.GenericTypeArguments[0];
			return $"array of {GetName(itemType)}";
		}
		return GetFriendlyTypeName(type);
	}

	private static string GetFriendlyTypeName(Type type, StringBuilder? sb = null)
	{
		sb ??= new StringBuilder();
		var name = type.Name;
		if (!type.IsGenericType)
		{
			if (type is { IsNested: true, IsGenericParameter: false })
				name = $"{name} in {GetName(type.DeclaringType!)}";
			return name;
		}

		sb.Append(name[..name.IndexOf('`')]);
		sb.Append(" of ");
		sb.Append(string.Join(" and ", type.GetGenericArguments()
			.Select(x => GetFriendlyTypeName(x, sb))));
		name = sb.ToString();

		if (type.IsNested)
			name = $"{name} in {GetName(type.DeclaringType!)}";

		return name;
	}

	internal static bool IsMarkedAsNullable(this MemberInfo member)
	{
#if NET8_0_OR_GREATER
		var infoContext = member switch
		{
			PropertyInfo p => new NullabilityInfoContext().Create(p),
			FieldInfo f => new NullabilityInfoContext().Create(f),
			_ => null
		};

		return infoContext?.WriteState is NullabilityState.Nullable;
#else
		return false;
#endif
	}
}