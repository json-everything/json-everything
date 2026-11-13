using System;
using Json.More;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `exclusiveMinimum` keyword.
/// </summary>
/// <remarks>
/// The `value` parameter is provided in the constructor as a `double` but stored as a `decimal`
/// because `decimal` is not a valid attribute parameter type.
/// As such, to prevent overflows, the value is clamped to the `decimal` range prior to being converted.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
                AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class ExclusiveMinimumAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// The exclusive minimum.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int GenericParameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="ExclusiveMinimumAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="useNet10CompatibleRounding">(.Net 11 only) <c>>true</c> forces .Net 10 and earlier rounding from <c>>double</c> to <c>>decimal</c></param>
	/// <remarks>
	/// The <paramref name="value"/> parameter is provided as a `double` but stored as a `decimal`
	/// because `decimal` is not a valid attribute parameter type.
	/// As such, to prevent overflows, the value is clamped to the `decimal` range prior to being converted.
	/// </remarks>
	public ExclusiveMinimumAttribute(double value, bool useNet10CompatibleRounding = false)
	{
		Value = value.ConvertToDecimal(useNet10CompatibleRounding);
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber() && !context.Type.IsNullableNumber()) return;

		context.Intents.Add(new ExclusiveMinimumIntent(Value));
	}
}