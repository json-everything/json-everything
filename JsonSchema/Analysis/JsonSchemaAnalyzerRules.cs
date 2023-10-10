namespace Json.Schema.Analysis;

public static class JsonSchemaAnalyzerRules
{
	public static readonly IRule MissingType =
		new MetaSchemaRule
		{
			MetaSchema =
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.Properties(
						(TypeKeyword.Name, false)
					)
					.MinProperties(1)
					.Unrecognized("x-diagnostic-message", "Non-empty object schemas should have a `type` keyword.\"")
		};

	public static readonly IRule MissingContains =
		new MetaSchemaRule
		{
			MetaSchema =
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.Properties(
						(ContainsKeyword.Name, false)
					)
					.AnyOf(
						new JsonSchemaBuilder()
							.Required(MinContainsKeyword.Name)
							.Unrecognized("x-diagnostic-message", "The `minContains` keyword is ineffective without the `contains` keyword."),
						new JsonSchemaBuilder()
							.Required(MaxContainsKeyword.Name)
							.Unrecognized("x-diagnostic-message", "The `maxContains` keyword is ineffective without the `contains` keyword.")
					)
		};
}