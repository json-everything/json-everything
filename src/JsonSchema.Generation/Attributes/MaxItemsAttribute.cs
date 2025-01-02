using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `maxItems` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class MaxItemsAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// The maximum number of items.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int Parameter { get; set; } = -1;

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
