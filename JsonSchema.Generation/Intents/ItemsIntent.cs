namespace Json.Schema.Generation.Intents
{
	internal class ItemsIntent : ISchemaKeywordIntent
	{
		public SchemaGeneratorContext Context { get; set; }

		public ItemsIntent(SchemaGeneratorContext context)
		{
			Context = context;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Items(Context.Apply());
		}
	}
}