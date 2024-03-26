using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public class MaxLengthAttributeHandler : IAttributeHandler<System.ComponentModel.DataAnnotations.MaxLengthAttribute>
{
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var maxLength = (System.ComponentModel.DataAnnotations.MaxLengthAttribute)attribute;

		context.Intents.Add(new MaxLengthIntent((uint)maxLength.Length));
	}
}