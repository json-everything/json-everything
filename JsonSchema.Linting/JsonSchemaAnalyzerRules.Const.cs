namespace Json.Schema.Linting;

public static partial class JsonSchemaAnalyzerRules
{
	public static readonly JsonSchema IsolatedConst =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:isolated-const")
			.Type(SchemaValueType.Object)
			.Properties(
				(ConstKeyword.Name, true)
			)
			.Required(ConstKeyword.Name)
			.AnyOf(
				new JsonSchemaBuilder()
					.Required(PropertiesKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(PropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(PatternPropertiesKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(PatternPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(AdditionalPropertiesKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(AdditionalPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(UnevaluatedPropertiesKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(UnevaluatedPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinPropertiesKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MinPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxPropertiesKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MaxPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(ItemsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(ItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(PrefixItemsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(PrefixItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(AdditionalItemsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(AdditionalItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(UnevaluatedItemsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(UnevaluatedItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinItemsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MinItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxItemsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MaxItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinContainsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MinContainsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxContainsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MaxContainsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(ContainsKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(ContainsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinLengthKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MinLengthKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxLengthKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MaxLengthKeyword.Name),
				new JsonSchemaBuilder()
					.Required(PatternKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(PatternKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinimumKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MinimumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaximumKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MaximumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(ExclusiveMinimumKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(ExclusiveMinimumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(ExclusiveMaximumKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(ExclusiveMaximumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MultipleOfKeyword.Name)
					.DiagnosticMessage("`const` completely constrains the value.  No further constraints are needed.")
					.DiagnosticTarget(MultipleOfKeyword.Name)
			);
}