using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public class EnumDataTypeAttributeHandler : IAttributeHandler<EnumDataTypeAttribute>
{
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var enumDataType = (EnumDataTypeAttribute)attribute;

		var values = Enum.GetNames(enumDataType.EnumType).ToList();

		context.Intents.Add(new EnumIntent(values));
	}
}