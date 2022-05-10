using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `maxItems` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MaxItemsAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The maximum number of items.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaxItemsAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MaxItemsAttribute(uint value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsArray()) return;

		context.Intents.Add(new MaxItemsIntent(Value));
	}
}