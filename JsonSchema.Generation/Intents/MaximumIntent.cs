namespace Json.Schema.Generation.Intents
{
	internal class MaximumIntent : ISchemaKeywordIntent
	{
		public decimal Value { get; }

		public MaximumIntent(decimal value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Maximum(Value);
		}
	}
}