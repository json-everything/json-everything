namespace Json.Schema.Generation.Intents
{
	internal class TypeIntent : ISchemaKeywordIntent
	{
		public SchemaValueType Type { get; set; }

		public TypeIntent(SchemaValueType type)
		{
			Type = type;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Type(Type);
		}
	}
}