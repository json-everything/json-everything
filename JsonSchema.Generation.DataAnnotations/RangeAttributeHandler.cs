using System;
using System.ComponentModel.DataAnnotations;
using Json.More;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public class RangeAttributeHandler : IAttributeHandler<RangeAttribute>
{
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (!context.Type.IsNumber() && !context.Type.IsNullableNumber()) return;

		var range = (RangeAttribute)attribute;
		if (!range.OperandType.IsNumber() && !range.OperandType.IsNullableNumber()) return;

		context.Intents.Add(new MinimumIntent(Convert.ToDecimal(range.Minimum)));
		context.Intents.Add(new MaximumIntent(Convert.ToDecimal(range.Maximum)));
	}
}