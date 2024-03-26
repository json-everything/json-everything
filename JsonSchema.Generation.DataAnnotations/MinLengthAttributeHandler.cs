using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public class MinLengthAttributeHandler : IAttributeHandler<System.ComponentModel.DataAnnotations.MinLengthAttribute>
{
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var minLength = (System.ComponentModel.DataAnnotations.MinLengthAttribute)attribute;

		context.Intents.Add(new MinLengthIntent((uint)minLength.Length));
	}
}