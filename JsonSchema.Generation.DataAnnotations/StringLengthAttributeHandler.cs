using System;
using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public class StringLengthAttributeHandler : IAttributeHandler<StringLengthAttribute>
{
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var length = (StringLengthAttribute)attribute;

		if (length.MinimumLength != 0)
			context.Intents.Add(new MinLengthIntent((uint)length.MinimumLength));
		context.Intents.Add(new MaxLengthIntent((uint)length.MaximumLength));
	}
}
