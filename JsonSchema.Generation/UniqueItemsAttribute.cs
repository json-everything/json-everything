using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `uniqueItems` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class UniqueItemsAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// Whether the items should be unique.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="UniqueItemsAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public UniqueItemsAttribute(bool value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsArray() || context.Type == typeof(string)) return;

		context.Intents.Add(new UniqueItemsIntent(Value));
	}
}