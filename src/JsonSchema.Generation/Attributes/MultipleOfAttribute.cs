using System;
using Json.More;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `multipleOf` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class MultipleOfAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// The divisor.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int GenericParameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="MultipleOfAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="useNet10CompatibleRounding">(.Net 11 only) <c>>true</c> forces .Net 10 and earlier rounding from <c>>double</c> to <c>>decimal</c></param>
	public MultipleOfAttribute(double value, bool useNet10CompatibleRounding = false)
	{
		Value = value.ConvertToDecimal(useNet10CompatibleRounding);
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber() && !context.Type.IsNullableNumber()) return;

		context.Intents.Add(new MultipleOfIntent(Value));
	}
}