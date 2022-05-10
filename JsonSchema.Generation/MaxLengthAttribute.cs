using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `maxLength` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MaxLengthAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The maximum length.
	/// </summary>
	public uint Length { get; }

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