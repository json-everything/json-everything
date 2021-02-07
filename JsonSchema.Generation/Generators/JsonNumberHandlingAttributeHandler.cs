using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation.Generators
{
	public class JsonNumberHandlingAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<JsonNumberHandlingAttribute>().FirstOrDefault();
			if (attribute == null) return;
		}
	}
}