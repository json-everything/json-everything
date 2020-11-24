namespace Json.Schema.Generation.Intents
{
	internal class MinItemsIntent : ISchemaKeywordIntent
	{
		public uint Value { get; }

		public MinItemsIntent(uint value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.MinItems(Value);
		}
	}
}