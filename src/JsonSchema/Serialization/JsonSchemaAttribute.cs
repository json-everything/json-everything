using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Json.Schema.Serialization;

/// <summary>
/// Identifies a <see cref="JsonSchema"/> to use when deserializing a type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public class JsonSchemaAttribute : Attribute
{
	internal JsonSchemaNode Schema { get; }

	/// <summary>
	/// Identifies a <see cref="JsonSchema"/> to use when deserializing a type.
	/// </summary>
	/// <param name="declaringType">The type declaring the schema.</param>
	/// <param name="memberName">The property or field name for the schema.  This member must be public and static.</param>
	/// <exception cref="ArgumentException">Thrown when the member cannot be found or is not public and static.</exception>
	[RequiresUnreferencedCode("Uses reflection")]
	public JsonSchemaAttribute(Type declaringType, string memberName)
	{
		MemberInfo? member = declaringType.GetProperty(memberName, BindingFlags.Static | BindingFlags.Public);
		if (member == null)
		{
			member = declaringType.GetField(memberName, BindingFlags.Static | BindingFlags.Public);
			if (member == null)
				throw new ArgumentException($"Cannot find public static member named `{memberName}`");
		}

		var prop = member as PropertyInfo;
		var field = member as FieldInfo;

		var value = prop?.GetValue(null) ?? field?.GetValue(null);
		if (value is not JsonSchemaNode schema)
			throw new ArgumentException($"Value of property must be `{typeof(JsonSchema).FullName}`");

		Schema = schema;
	}
}