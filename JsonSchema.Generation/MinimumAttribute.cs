using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `minimum` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MinimumAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The minimum.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="MinimumAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MinimumAttribute(double value)
	{
		Value = Convert.ToDecimal(value);
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber()) return;

		context.Intents.Add(new MinimumIntent(Value));
	}
}