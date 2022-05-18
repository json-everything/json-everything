using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `exclusiveMaximum` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class ExclusiveMaximumAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The exclusive maximum.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="ExclusiveMaximumAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ExclusiveMaximumAttribute(double value)
	{
		Value = Convert.ToDecimal(value);
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber()) return;

		context.Intents.Add(new ExclusiveMaximumIntent(Value));
	}
}