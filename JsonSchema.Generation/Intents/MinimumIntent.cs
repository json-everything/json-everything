namespace Json.Schema.Generation.Intents
{
	internal class MinimumIntent : ISchemaKeywordIntent
	{
		public decimal Value { get; }

		public MinimumIntent(decimal value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Minimum(Value);
		}
	}
}