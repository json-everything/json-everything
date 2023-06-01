using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `minItems` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MinItemsAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The minimum number of items.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MinItemsAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MinItemsAttribute(uint value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsArray()) return;

		context.Intents.Add(new MinItemsIntent(Value));
	}
}