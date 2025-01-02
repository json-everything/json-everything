using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `additionalProperties` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class AdditionalItemsAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
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
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int GenericParameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="AdditionalPropertiesAttribute"/> instance.
	/// </summary>
	/// <param name="boolSchema">A boolean schema.</param>
	public AdditionalItemsAttribute(bool boolSchema)
	{
		BoolValue = boolSchema;
	}

	/// <summary>
	/// Creates a new <see cref="AdditionalPropertiesAttribute"/> instance.
	/// </summary>
	/// <param name="typeSchema">A type to generate a schema for the keyword.</param>
	public AdditionalItemsAttribute(Type typeSchema)
	{
		TypeValue = typeSchema;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsArray()) return;

		if (BoolValue.HasValue)
		{
			context.Intents.Add(new AdditionalItemsIntent(BoolValue.Value
				? SchemaGenerationContextBase.True
				: SchemaGenerationContextBase.False));
			return;
		}

		context.Intents.Add(new AdditionalItemsIntent(SchemaGenerationContextCache.Get(TypeValue!)));
	}
}