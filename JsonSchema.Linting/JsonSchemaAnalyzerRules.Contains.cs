namespace Json.Schema.Linting;

public static partial class JsonSchemaAnalyzerRules
{
	public static readonly JsonSchema MissingContains =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:missing-type")
			.Type(SchemaValueType.Object)
			.Properties(
				(ContainsKeyword.Name, false)
			)
			.AnyOf(
				new JsonSchemaBuilder()
					.Required(MinContainsKeyword.Name)
					.DiagnosticMessage("Keyword is ineffective without the `contains` keyword.")
					.DiagnosticTarget(MinContainsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxContainsKeyword.Name)
					.DiagnosticMessage("Keyword is ineffective without the `contains` keyword.")
					.DiagnosticTarget(MaxContainsKeyword.Name)
			);
}