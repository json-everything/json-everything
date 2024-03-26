using System;
using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public class RegularExpressionAttributeHandler : IAttributeHandler<RegularExpressionAttribute>
{
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var regex = (RegularExpressionAttribute)attribute;

		context.Intents.Add(new PatternIntent(regex.Pattern));
	}
}