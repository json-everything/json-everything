using System;
using System.Text.Json.Nodes;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `default` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
                AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class DefaultAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// The value.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int GenericParameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(int value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(uint value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(long value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(ulong value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(float value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(double value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(string? value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="DefaultAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultAttribute(bool value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new DefaultIntent(Value));
	}
}