#if NET8_0_OR_GREATER

using System;
using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public class LengthAttributeHandler : IAttributeHandler<LengthAttribute>
{
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var length = (LengthAttribute)attribute;

		context.Intents.Add(new MinLengthIntent((uint)length.MinimumLength));
		context.Intents.Add(new MaxLengthIntent((uint)length.MaximumLength));
	}
}

#endif