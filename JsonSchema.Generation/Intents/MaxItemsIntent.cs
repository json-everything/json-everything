namespace Json.Schema.Generation.Intents
{
	internal class MaxItemsIntent : ISchemaKeywordIntent
	{
		public uint Value { get; }

		public MaxItemsIntent(uint value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.MaxItems(Value);
		}
	}
}