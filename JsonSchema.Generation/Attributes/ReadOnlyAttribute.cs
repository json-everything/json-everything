using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `readOnly` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class ReadOnlyAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// Whether the property should be read-only.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="ReadOnlyAttribute"/> instance with a value of `true`.
	/// </summary>
	public ReadOnlyAttribute()
		: this(true)
	{

	}

	/// <summary>
	/// Creates a new <see cref="ReadOnlyAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ReadOnlyAttribute(bool value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new ReadOnlyIntent(Value));
	}
}