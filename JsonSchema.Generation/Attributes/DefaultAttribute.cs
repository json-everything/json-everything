using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `default` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
                AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class DefaultAttribute : ConditionalAttribute, IAttributeHandler
{
	/// <summary>
	/// The value.
	/// </summary>
	public object? Value { get; }

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(object? value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new DefaultIntent(Value));
	}
}