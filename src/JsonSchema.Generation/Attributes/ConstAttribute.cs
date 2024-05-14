using System;
using System.Text.Json.Nodes;
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
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(int value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(uint value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(long value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(ulong value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(float value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(double value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(string? value)
	{
		Value = value;
	}

	/// <summary>
	/// Creates a new <see cref="ConstAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstAttribute(bool value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new ConstIntent(Value));
	}
}