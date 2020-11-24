namespace Json.Schema.Generation.Intents
{
	internal class UniqueItemsIntent : ISchemaKeywordIntent
	{
		public bool Value { get; }

		public UniqueItemsIntent(bool value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.UniqueItems(Value);
		}
	}
}