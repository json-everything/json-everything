using System.Linq;
using System.Reflection;

namespace Json.Schema.Analysis;

public static class JsonSchemaAnalyzerRules
{
	internal static readonly JsonSchema[] DefinedMetaSchemas;
	internal static readonly IRule[] DefinedRules;

	static JsonSchemaAnalyzerRules()
	{
		DefinedMetaSchemas = typeof(JsonSchemaAnalyzerRules)
			.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			.Where(x => x.FieldType == typeof(JsonSchema))
			.Select(x => (JsonSchema)x.GetValue(null))
			.ToArray();
		DefinedRules = typeof(JsonSchemaAnalyzerRules)
			.GetFields(BindingFlags.Static | BindingFlags.Public)
			.Where(x => x.IsPublic && x.FieldType == typeof(JsonSchema))
			.Select(x => (IRule)new MetaSchemaRule((JsonSchema)x.GetValue(null)))
			.ToArray();
	}

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