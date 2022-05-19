using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `additionalProperties` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
                AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class AdditionalPropertiesAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// If the attribute value represents a boolean schema, gets the boolean value.
	/// </summary>
	public bool? BoolValue { get; }
	/// <summary>
	/// If the attribute value represents a type schema, gets the type.
	/// </summary>
	public Type? TypeValue { get; }

	/// <summary>
	/// Creates a new <see cref="AdditionalPropertiesAttribute"/> instance.
	/// </summary>
	/// <param name="boolSchema">A boolean schema.</param>
	public AdditionalPropertiesAttribute(bool boolSchema)
	{
		BoolValue = boolSchema;
	}

	/// <summary>
	/// Creates a new <see cref="AdditionalPropertiesAttribute"/> instance.
	/// </summary>
	/// <param name="typeSchema">A type to generate the a schema for the keyword.</param>
	public AdditionalPropertiesAttribute(Type typeSchema)
	{
		TypeValue = typeSchema;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (BoolValue.HasValue)
		{
			context.Intents.Add(new AdditionalPropertiesIntent(BoolValue.Value
				? SchemaGenerationContextBase.True
				: SchemaGenerationContextBase.False));
			return;
		}

		context.Intents.Add(new AdditionalPropertiesIntent(SchemaGenerationContextCache.Get(TypeValue!)));
	}
}