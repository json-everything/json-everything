using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `minimum` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MinLengthAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The minimum length.
	/// </summary>
	public uint Length { get; }

	/// <summary>
	/// Creates a new <see cref="MinLengthAttribute"/> instance.
	/// </summary>
	/// <param name="length">The value.</param>
	public MinLengthAttribute(uint length)
	{
		Length = length;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (context.Type != typeof(string)) return;

		context.Intents.Add(new MinLengthIntent(Length));
	}
}