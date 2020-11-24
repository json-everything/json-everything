namespace Json.Schema.Generation.Intents
{
	internal class MultipleOfIntent : ISchemaKeywordIntent
	{
		public decimal Value { get; }

		public MultipleOfIntent(decimal value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.MultipleOf(Value);
		}
	}
}