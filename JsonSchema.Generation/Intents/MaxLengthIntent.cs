namespace Json.Schema.Generation.Intents
{
	internal class MaxLengthIntent : ISchemaKeywordIntent
	{
		public uint Value { get; }

		public MaxLengthIntent(uint value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.MaxLength(Value);
		}
	}
}