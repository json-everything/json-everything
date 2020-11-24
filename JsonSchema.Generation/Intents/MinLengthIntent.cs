namespace Json.Schema.Generation.Intents
{
	internal class MinLengthIntent : ISchemaKeywordIntent
	{
		public uint Value { get; }

		public MinLengthIntent(uint value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.MinLength(Value);
		}
	}
}