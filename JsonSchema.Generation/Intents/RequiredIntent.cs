using System.Collections.Generic;

namespace Json.Schema.Generation.Intents
{
	internal class RequiredIntent : ISchemaKeywordIntent
	{
		public List<string> RequiredProperties { get; set; }

		public RequiredIntent(List<string> requiredProperties)
		{
			RequiredProperties = requiredProperties;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Required(RequiredProperties);
		}
	}
}