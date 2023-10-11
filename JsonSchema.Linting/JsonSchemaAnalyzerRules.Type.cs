namespace Json.Schema.Linting;

public static partial class JsonSchemaAnalyzerRules
{
	public static readonly JsonSchema MissingType =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:missing-type")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, false),
				(ConstKeyword.Name, false),
				(EnumKeyword.Name, false)
			)
			.MinProperties(1)
			.DiagnosticMessage("Non-empty object schemas should have a `type` keyword.\"");

	private static readonly JsonSchema DisallowedObjectKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:disallowed-object")
			.AnyOf(
				new JsonSchemaBuilder()
					.Required(PropertiesKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `object`.")
					.DiagnosticTarget(PropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(PatternPropertiesKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `object`.")
					.DiagnosticTarget(PatternPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(AdditionalPropertiesKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `object`.")
					.DiagnosticTarget(AdditionalPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(UnevaluatedPropertiesKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `object`.")
					.DiagnosticTarget(UnevaluatedPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinPropertiesKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `object`.")
					.DiagnosticTarget(MinPropertiesKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxPropertiesKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `object`.")
					.DiagnosticTarget(MaxPropertiesKeyword.Name)
			);

	private static readonly JsonSchema DisallowedArrayKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:disallowed-array")
			.AnyOf(
				new JsonSchemaBuilder()
					.Required(ItemsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(ItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(PrefixItemsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(PrefixItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(AdditionalItemsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(AdditionalItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(UnevaluatedItemsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(UnevaluatedItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinItemsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(MinItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxItemsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(MaxItemsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MinContainsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(MinContainsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxContainsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(MaxContainsKeyword.Name),
				new JsonSchemaBuilder()
					.Required(ContainsKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `array`.")
					.DiagnosticTarget(ContainsKeyword.Name)
			);

	private static readonly JsonSchema DisallowedStringKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:disallowed-string")
			.AnyOf(
				new JsonSchemaBuilder()
					.Required(MinLengthKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `string`.")
					.DiagnosticTarget(MinLengthKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaxLengthKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `string`.")
					.DiagnosticTarget(MaxLengthKeyword.Name),
				new JsonSchemaBuilder()
					.Required(PatternKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `string`.")
					.DiagnosticTarget(PatternKeyword.Name)
			);

	private static readonly JsonSchema DisallowedNumberKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:disallowed-number")
			.AnyOf(
				new JsonSchemaBuilder()
					.Required(MinimumKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `number` or `integer`.")
					.DiagnosticTarget(MinimumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaximumKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `number` or `integer`.")
					.DiagnosticTarget(MaximumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(ExclusiveMinimumKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `number` or `integer`.")
					.DiagnosticTarget(ExclusiveMinimumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(ExclusiveMaximumKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `number` or `integer`.")
					.DiagnosticTarget(ExclusiveMaximumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MultipleOfKeyword.Name)
					.DiagnosticMessage("Keyword is only effective when `type` is `number` or `integer`.")
					.DiagnosticTarget(MultipleOfKeyword.Name)
			);

	public static readonly JsonSchema NonObjectKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:non-object")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("object"))
			)
			.Required(TypeKeyword.Name)
			.AnyOf(
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-array"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-string"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-number")
			);

	public static readonly JsonSchema NonArrayKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:non-array")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("array"))
			)
			.Required(TypeKeyword.Name)
			.AnyOf(
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-object"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-string"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-number")
			);

	public static readonly JsonSchema NonStringKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:non-array")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("string"))
			)
			.Required(TypeKeyword.Name)
			.AnyOf(
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-object"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-array"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-number")
			);

	public static readonly JsonSchema NonNumberKeywords =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("schema:json-everything:analysis:non-array")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Enum("number", "integer"))
			)
			.Required(TypeKeyword.Name)
			.AnyOf(
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-object"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-array"),
				new JsonSchemaBuilder().Ref("schema:json-everything:analysis:disallowed-string")
			);
}