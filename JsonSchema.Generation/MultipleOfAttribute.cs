using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `multipleOf` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MultipleOfAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The divisor.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="MultipleOfAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MultipleOfAttribute(double value)
	{
		Value = (decimal)value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber()) return;

		context.Intents.Add(new MultipleOfIntent(Value));
	}
}