namespace Json.Schema.Generation.Intents
{
	internal class AdditionalPropertiesIntent : ISchemaKeywordIntent
	{
		public SchemaGeneratorContext Context { get; set; }

		public AdditionalPropertiesIntent(SchemaGeneratorContext context)
		{
			Context = context;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.AdditionalProperties(Context.Apply());
		}
	}
}