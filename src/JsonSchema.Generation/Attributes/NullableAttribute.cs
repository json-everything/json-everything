using System;

namespace Json.Schema.Generation;

/// <summary>
/// Overrides the nullability declared in code and either adds or removes `null` in the `type` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class NullableAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// Gets whether `null` should be included in the `type` keyword.
	/// </summary>
	public bool IsNullable { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int Parameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="NullableAttribute"/> instance.
	/// </summary>
	/// <param name="isNullable">Whether `null` should be included in the `type` keyword.</param>
	public NullableAttribute(bool isNullable)
	{
		IsNullable = isNullable;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
	}
}