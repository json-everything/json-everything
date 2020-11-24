namespace Json.Schema.Generation.Intents
{
	internal class ExclusiveMaximumIntent : ISchemaKeywordIntent
	{
		public decimal Value { get; }

		public ExclusiveMaximumIntent(decimal value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.ExclusiveMaximum(Value);
		}
	}
}