using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `writeOnly` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class WriteOnlyAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// Whether the property should be write-only.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int Parameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="WriteOnlyAttribute"/> instance with a value of `true`.
	/// </summary>
	public WriteOnlyAttribute()
		: this(true)
	{
	}

	/// <summary>
	/// Creates a new <see cref="WriteOnlyAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public WriteOnlyAttribute(bool value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new WriteOnlyIntent(Value));
	}
}