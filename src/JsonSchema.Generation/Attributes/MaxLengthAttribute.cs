using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `maxLength` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class MaxLengthAttribute : ConditionalAttribute,INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// The maximum length.
	/// </summary>
	public uint Length { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int GenericParameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="MaxLengthAttribute"/> instance.
	/// </summary>
	/// <param name="length">The value.</param>
	public MaxLengthAttribute(uint length)
	{
		Length = length;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (context.Type != typeof(string)) return;

		context.Intents.Add(new MaxLengthIntent(Length));
	}
}