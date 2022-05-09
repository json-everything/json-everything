using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `exclusiveMinimum` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExclusiveMinimumAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The exclusive minimum.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="ExclusiveMinimumAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ExclusiveMinimumAttribute(double value)
	{
		Value = Convert.ToDecimal(value);
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber()) return;

		context.Intents.Add(new ExclusiveMinimumIntent(Value));
	}
}