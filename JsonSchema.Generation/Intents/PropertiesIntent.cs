using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	internal class PropertiesIntent : ISchemaKeywordIntent
	{
		public Dictionary<string, SchemaGeneratorContext> Properties { get; set; }

		public PropertiesIntent(Dictionary<string, SchemaGeneratorContext> properties)
		{
			Properties = properties;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Properties(Properties.ToDictionary(p => p.Key, p => p.Value.Apply().Build()));
		}
	}
}