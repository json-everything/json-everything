using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

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
		return type.IsArray ||
			   type == typeof(Array) ||
			   typeof(IEnumerable).IsAssignableFrom(type);
	}

	internal static int GetAttributeSetHashCode(this IEnumerable<Attribute> items)
	{
		var eligible = items.Where(a => a is not JsonPropertyNameAttribute and not RequiredAttribute)
			.OrderBy(x => x.GetType().AssemblyQualifiedName);
		unchecked
		{
			var hashCode = 0;
			foreach (var attribute in eligible)
			{
				var properties = attribute.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.OrderBy(x => x.Name);
				var propertyHash = properties.Aggregate(0, (c, p) => (c * 397) ^ (p.GetValue(attribute)?.GetHashCode() ?? 0));

				hashCode = (hashCode * 397) ^ attribute.GetType().GetHashCode();
				hashCode = (hashCode * 397) ^ propertyHash;
			}
			return hashCode;
		}
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

	internal static IEnumerable<Type> GetUsedTypes(this Type type)
	{
		var usedTypes = new HashSet<Type>{type};

		GetUsedTypes(type, usedTypes);

		return usedTypes;
	}

	private static void GetUsedTypes(this Type type, HashSet<Type> usedTypes)
	{
		var propertiesToGenerate = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var fieldsToGenerate = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
		var hiddenPropertiesToGenerate = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(p => p.GetCustomAttribute<JsonIncludeAttribute>() != null);
		var hiddenFieldsToGenerate = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(p => p.GetCustomAttribute<JsonIncludeAttribute>() != null);
		var membersToGenerate = propertiesToGenerate.Cast<MemberInfo>()
			.Concat(fieldsToGenerate)
			.Concat(hiddenPropertiesToGenerate)
			.Concat(hiddenFieldsToGenerate);

		foreach (var memberInfo in membersToGenerate)
		{
			var memberType = GetMemberType(memberInfo);

			if (usedTypes.Add(memberType))
				GetUsedTypes(memberType, usedTypes);
		}
	}

}