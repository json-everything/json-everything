using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `maximum` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MaximumAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The maximum.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaximumAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MaximumAttribute(double value)
	{
		Value = Convert.ToDecimal(value);
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber()) return;

		context.Intents.Add(new MaximumIntent(Value));
	}
}