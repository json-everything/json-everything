using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Json.Schema;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static partial class JsonSchemaExtensions
{
	/// <summary>
	/// Gets the schema for `additionalItems` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetAdditionalItems(this JsonSchema schema)
	{
		return schema.TryGetKeyword<AdditionalItemsKeyword>(AdditionalItemsKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the schema for `additionalProperties` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetAdditionalProperties(this JsonSchema schema)
	{
		return schema.TryGetKeyword<AdditionalPropertiesKeyword>(AdditionalPropertiesKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the schemas in `allOf` if the keyword exists.
	/// </summary>
	public static IReadOnlyList<JsonSchema>? GetAllOf(this JsonSchema schema)
	{
		return schema.TryGetKeyword<AllOfKeyword>(AllOfKeyword.Name, out var k) ? k.Schemas : null;
	}

	/// <summary>
	/// Gets the value of `$anchor` if the keyword exists.
	/// </summary>
	public static string? GetAnchor(this JsonSchema schema)
	{
		return schema.TryGetKeyword<AnchorKeyword>(AnchorKeyword.Name, out var k) ? k.Anchor : null;
	}

	/// <summary>
	/// Gets the schemas in `anyOf` if the keyword exists.
	/// </summary>
	public static IReadOnlyList<JsonSchema>? GetAnyOf(this JsonSchema schema)
	{
		return schema.TryGetKeyword<AnyOfKeyword>(AnyOfKeyword.Name, out var k) ? k.Schemas : null;
	}

	/// <summary>
	/// Gets the value of `$comment` if the keyword exists.
	/// </summary>
	public static string? GetComment(this JsonSchema schema)
	{
		return schema.TryGetKeyword<CommentKeyword>(CommentKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `const` if the keyword exists.
	/// </summary>
	public static JsonNode? GetConst(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ConstKeyword>(ConstKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the schema for `contains` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetContains(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ContainsKeyword>(ContainsKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the value of `contentEncoding` if the keyword exists.
	/// </summary>
	public static string? GetContentEncoding(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ContentEncodingKeyword>(ContentEncodingKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `contentMediaType` if the keyword exists.
	/// </summary>
	public static string? GetContentMediaType(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ContentMediaTypeKeyword>(ContentMediaTypeKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the schema for `contentSchema` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetContentSchema(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ContentSchemaKeyword>(ContentSchemaKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the value of `default` if the keyword exists.
	/// </summary>
	public static JsonNode? GetDefault(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DefaultKeyword>(DefaultKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the schemas in `definitions` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, JsonSchema>? GetDefinitions(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DefinitionsKeyword>(DefinitionsKeyword.Name, out var k) ? k.Definitions : null;
	}

	/// <summary>
	/// Gets the schemas in `$defs` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, JsonSchema>? GetDefs(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DefsKeyword>(DefsKeyword.Name, out var k) ? k.Definitions : null;
	}

	/// <summary>
	/// Gets the requirements in `dependencies` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, SchemaOrPropertyList>? GetDependencies(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DependenciesKeyword>(DependenciesKeyword.Name, out var k) ? k.Requirements : null;
	}

	/// <summary>
	/// Gets the requirements in `dependentRequired` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, IReadOnlyList<string>>? GetDependentRequired(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DependentRequiredKeyword>(DependentRequiredKeyword.Name, out var k) ? k.Requirements : null;
	}

	/// <summary>
	/// Gets the schemas in `dependentSchemas` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, JsonSchema>? GetDependentSchemas(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DependentSchemasKeyword>(DependentSchemasKeyword.Name, out var k) ? k.Schemas : null;
	}

	/// <summary>
	/// Gets the value of `deprecated` if the keyword exists.
	/// </summary>
	public static bool? GetDeprecated(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DeprecatedKeyword>(DeprecatedKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `description` if the keyword exists.
	/// </summary>
	public static string? GetDescription(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DescriptionKeyword>(DescriptionKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `$dynamicAnchor` if the keyword exists.
	/// </summary>
	public static string? GetDynamicAnchor(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DynamicAnchorKeyword>(DynamicAnchorKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `$dynamicRef` if the keyword exists.
	/// </summary>
	public static Uri? GetDynamicRef(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DynamicRefKeyword>(DynamicRefKeyword.Name, out var k) ? k.Reference : null;
	}

	/// <summary>
	/// Gets the schema for `else` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetElse(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ElseKeyword>(ElseKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the values in `enum` if the keyword exists.
	/// </summary>
	public static IReadOnlyCollection<JsonNode?>? GetEnum(this JsonSchema schema)
	{
		return schema.TryGetKeyword<EnumKeyword>(EnumKeyword.Name, out var k) ? k.Values : null;
	}

	/// <summary>
	/// Gets the values in `examples` if the keyword exists.
	/// </summary>
	public static IReadOnlyList<JsonNode?>? GetExamples(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ExamplesKeyword>(ExamplesKeyword.Name, out var k) ? k.Values : null;
	}

	/// <summary>
	/// Gets the value of `exclusiveMaximum` if the keyword exists.
	/// </summary>
	public static decimal? GetExclusiveMaximum(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ExclusiveMaximumKeyword>(ExclusiveMaximumKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `exclusiveMinimum` if the keyword exists.
	/// </summary>
	public static decimal? GetExclusiveMinimum(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ExclusiveMinimumKeyword>(ExclusiveMinimumKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `format` if the keyword exists.
	/// </summary>
	public static Format? GetFormat(this JsonSchema schema)
	{
		return schema.TryGetKeyword<FormatKeyword>(FormatKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `$id` if the keyword exists.
	/// </summary>
	public static Uri? GetId(this JsonSchema schema)
	{
		return schema.TryGetKeyword<IdKeyword>(IdKeyword.Name, out var k) ? k.Id : null;
	}

	/// <summary>
	/// Gets the schema for `if` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetIf(this JsonSchema schema)
	{
		return schema.TryGetKeyword<IfKeyword>(IfKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the schema for `items` if the keyword exists and is a single schema.
	/// </summary>
	public static JsonSchema? GetItems(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ItemsKeyword>(ItemsKeyword.Name, out var k) ? k.SingleSchema : null;
	}

	/// <summary>
	/// Gets the schemas in `items` if the keyword exists and is an array of schemas.
	/// </summary>
	public static IReadOnlyList<JsonSchema>? GetItemsArrayForm(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ItemsKeyword>(ItemsKeyword.Name, out var k) ? k.ArraySchemas : null;
	}

	/// <summary>
	/// Gets the value of `maxContains` if the keyword exists.
	/// </summary>
	public static uint? GetMaxContains(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MaxContainsKeyword>(MaxContainsKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `maximum` if the keyword exists.
	/// </summary>
	public static decimal? GetMaximum(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MaximumKeyword>(MaximumKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `maxItems` if the keyword exists.
	/// </summary>
	public static uint? GetMaxItems(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MaxItemsKeyword>(MaxItemsKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `maxLength` if the keyword exists.
	/// </summary>
	public static uint? GetMaxLength(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MaxLengthKeyword>(MaxLengthKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `maxProperties` if the keyword exists.
	/// </summary>
	public static uint? GetMaxProperties(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MaxPropertiesKeyword>(MaxPropertiesKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `minContains` if the keyword exists.
	/// </summary>
	public static uint? GetMinContains(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MinContainsKeyword>(MinContainsKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `minimum` if the keyword exists.
	/// </summary>
	public static decimal? GetMinimum(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MinimumKeyword>(MinimumKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `minItems` if the keyword exists.
	/// </summary>
	public static uint? GetMinItems(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MinItemsKeyword>(MinItemsKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `minLength` if the keyword exists.
	/// </summary>
	public static uint? GetMinLength(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MinLengthKeyword>(MinLengthKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `minProperties` if the keyword exists.
	/// </summary>
	public static uint? GetMinProperties(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MinPropertiesKeyword>(MinPropertiesKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `multipleOf` if the keyword exists.
	/// </summary>
	public static decimal? GetMultipleOf(this JsonSchema schema)
	{
		return schema.TryGetKeyword<MultipleOfKeyword>(MultipleOfKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the schema for `not` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetNot(this JsonSchema schema)
	{
		return schema.TryGetKeyword<NotKeyword>(NotKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the schemas in `oneOf` if the keyword exists.
	/// </summary>
	public static IReadOnlyList<JsonSchema>? GetOneOf(this JsonSchema schema)
	{
		return schema.TryGetKeyword<OneOfKeyword>(OneOfKeyword.Name, out var k) ? k.Schemas : null;
	}

	/// <summary>
	/// Gets the value of `pattern` if the keyword exists.
	/// </summary>
	public static Regex? GetPattern(this JsonSchema schema)
	{
		return schema.TryGetKeyword<PatternKeyword>(PatternKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the schemas in `patternProperties` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<Regex, JsonSchema>? GetPatternProperties(this JsonSchema schema)
	{
		return schema.TryGetKeyword<PatternPropertiesKeyword>(PatternPropertiesKeyword.Name, out var k) ? k.Patterns : null;
	}

	/// <summary>
	/// Gets the schemas in `prefixItems` if the keyword exists.
	/// </summary>
	public static IReadOnlyList<JsonSchema>? GetPrefixItems(this JsonSchema schema)
	{
		return schema.TryGetKeyword<PrefixItemsKeyword>(PrefixItemsKeyword.Name, out var k) ? k.ArraySchemas : null;
	}

	/// <summary>
	/// Gets the schemas in `properties` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, JsonSchema>? GetProperties(this JsonSchema schema)
	{
		return schema.TryGetKeyword<PropertiesKeyword>(PropertiesKeyword.Name, out var k) ? k.Properties : null;
	}

	/// <summary>
	/// Gets the schemas for `propertyDependencies` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, PropertyDependency>? GetPropertyDependencies(this JsonSchema schema)
	{
		return schema.TryGetKeyword<PropertyDependenciesKeyword>(PropertyDependenciesKeyword.Name, out var k) ? k.Dependencies : null;
	}

	/// <summary>
	/// Gets the schema for `propertyNames` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetPropertyNames(this JsonSchema schema)
	{
		return schema.TryGetKeyword<PropertyNamesKeyword>(PropertyNamesKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the value of `readOnly` if the keyword exists.
	/// </summary>
	public static bool? GetReadOnly(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ReadOnlyKeyword>(ReadOnlyKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `$recursiveAnchor` if the keyword exists.
	/// </summary>
	public static bool? GetRecursiveAnchor(this JsonSchema schema)
	{
		return schema.TryGetKeyword<RecursiveAnchorKeyword>(RecursiveAnchorKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `$recursiveRef` if the keyword exists.
	/// </summary>
	public static Uri? GetRecursiveRef(this JsonSchema schema)
	{
		return schema.TryGetKeyword<RecursiveRefKeyword>(RecursiveRefKeyword.Name, out var k) ? k.Reference : null;
	}

	/// <summary>
	/// Gets the value of `$ref` if the keyword exists.
	/// </summary>
	public static Uri? GetRef(this JsonSchema schema)
	{
		return schema.TryGetKeyword<RefKeyword>(RefKeyword.Name, out var k) ? k.Reference : null;
	}

	/// <summary>
	/// Gets the values in `required` if the keyword exists.
	/// </summary>
	public static IReadOnlyList<string>? GetRequired(this JsonSchema schema)
	{
		return schema.TryGetKeyword<RequiredKeyword>(RequiredKeyword.Name, out var k) ? k.Properties : null;
	}

	/// <summary>
	/// Gets the value of `$schema` if the keyword exists.
	/// </summary>
	public static Uri? GetSchema(this JsonSchema schema)
	{
		return schema.TryGetKeyword<SchemaKeyword>(SchemaKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the schema for `then` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetThen(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ThenKeyword>(ThenKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the value of `title` if the keyword exists.
	/// </summary>
	public static string? GetTitle(this JsonSchema schema)
	{
		return schema.TryGetKeyword<TitleKeyword>(TitleKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the value of `type` if the keyword exists.
	/// </summary>
	public static SchemaValueType? GetJsonType(this JsonSchema schema)
	{
		return schema.TryGetKeyword<TypeKeyword>(TypeKeyword.Name, out var k) ? k.Type : null;
	}

	/// <summary>
	/// Gets the schema for `unevaluatedItems` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetUnevaluatedItems(this JsonSchema schema)
	{
		return schema.TryGetKeyword<UnevaluatedItemsKeyword>(UnevaluatedItemsKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the schema for `unevaluatedProperties` if the keyword exists.
	/// </summary>
	public static JsonSchema? GetUnevaluatedProperties(this JsonSchema schema)
	{
		return schema.TryGetKeyword<UnevaluatedPropertiesKeyword>(UnevaluatedPropertiesKeyword.Name, out var k) ? k.Schema : null;
	}

	/// <summary>
	/// Gets the value of `uniqueItems` if the keyword exists.
	/// </summary>
	public static bool? GetUniqueItems(this JsonSchema schema)
	{
		return schema.TryGetKeyword<UniqueItemsKeyword>(UniqueItemsKeyword.Name, out var k) ? k.Value : null;
	}

	/// <summary>
	/// Gets the values in `$vocabulary` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<Uri, bool>? GetVocabulary(this JsonSchema schema)
	{
		return schema.TryGetKeyword<VocabularyKeyword>(VocabularyKeyword.Name, out var k) ? k.Vocabulary : null;
	}

	/// <summary>
	/// Gets the value of `writeOnly` if the keyword exists.
	/// </summary>
	public static bool? GetWriteOnly(this JsonSchema schema)
	{
		return schema.TryGetKeyword<WriteOnlyKeyword>(WriteOnlyKeyword.Name, out var k) ? k.Value : null;
	}
}