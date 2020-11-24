namespace Json.Schema.Generation.Intents
{
	internal class ExclusiveMinimumIntent : ISchemaKeywordIntent
	{
		public decimal Value { get; }

		public ExclusiveMinimumIntent(decimal value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.ExclusiveMinimum(Value);
		}
	}
}