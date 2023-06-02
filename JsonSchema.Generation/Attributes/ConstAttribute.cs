using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `const` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
                AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class ConstAttribute : ConditionalAttribute, IAttributeHandler
{
	/// <summary>
	/// The value.
	/// </summary>
	public object? Value { get; }

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(object? value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new ConstIntent(Value));
	}
}