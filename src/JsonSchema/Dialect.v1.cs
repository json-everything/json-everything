using Json.Schema.Keywords;

namespace Json.Schema;

public partial class Dialect
{
	public static Dialect V1 { get; } = new(
		AdditionalPropertiesKeyword.Instance,
		AllOfKeyword.Instance,
		AnchorKeyword.Instance,
		AnyOfKeyword.Instance,
		CommentKeyword.Instance,
		ConstKeyword.Instance,
		ContainsKeyword.Instance,
		ContentEncodingKeyword.Instance,
		ContentMediaTypeKeyword.Instance,
		ContentSchemaKeyword.Instance,
		DefaultKeyword.Instance,
		DefsKeyword.Instance,
		DependentRequiredKeyword.Instance,
		DependentSchemasKeyword.Instance,
		DeprecatedKeyword.Instance,
		DescriptionKeyword.Instance,
		DynamicAnchorKeyword.Instance,
		DynamicRefKeyword.Instance,
		ElseKeyword.Instance,
		EnumKeyword.Instance,
		ExamplesKeyword.Instance,
		ExclusiveMaximumKeyword.Instance,
		ExclusiveMinimumKeyword.Instance,
		FormatKeyword.Instance,
		IdKeyword.Instance,
		IfKeyword.Instance,
		ItemsKeyword.Instance,
		MaxContainsKeyword.Instance,
		MaximumKeyword.Instance,
		MaxItemsKeyword.Instance,
		MaxLengthKeyword.Instance,
		MaxPropertiesKeyword.Instance,
		MinContainsKeyword.Instance,
		MinimumKeyword.Instance,
		MinItemsKeyword.Instance,
		MinLengthKeyword.Instance,
		MinPropertiesKeyword.Instance,
		MultipleOfKeyword.Instance,
		NotKeyword.Instance,
		OneOfKeyword.Instance,
		PatternKeyword.Instance,
		PatternPropertiesKeyword.Instance,
		PrefixItemsKeyword.Instance,
		PropertiesKeyword.Instance,
		// PropertyDependenciesKeyword.Instance,  // TODO: is proposal
		PropertyNamesKeyword.Instance,
		ReadOnlyKeyword.Instance,
		RefKeyword.Instance,
		RequiredKeyword.Instance,
		SchemaKeyword.Instance,
		ThenKeyword.Instance,
		TitleKeyword.Instance,
		TypeKeyword.Instance,
		UnevaluatedItemsKeyword.Instance,
		UnevaluatedPropertiesKeyword.Instance,
		UniqueItemsKeyword.Instance,
		WriteOnlyKeyword.Instance
	)
	{
		Id = MetaSchemas.Draft201909Id,
		_readOnly = true
	};
}