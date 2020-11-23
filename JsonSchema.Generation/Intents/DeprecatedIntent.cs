namespace Json.Schema.Generation.Intents
{
	internal class DeprecatedIntent : ISchemaKeywordIntent
	{
		public bool Deprecated { get; set; }

		public DeprecatedIntent(bool deprecated)
		{
			Deprecated = deprecated;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Deprecated(Deprecated);
		}
	}
}