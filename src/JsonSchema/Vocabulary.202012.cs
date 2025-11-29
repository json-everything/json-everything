using System;
using Json.Schema.Keywords;
// ReSharper disable InconsistentNaming

namespace Json.Schema;

public partial class Vocabulary
{
	public static Vocabulary Draft202012_Applicator { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"),
		AdditionalPropertiesKeyword.Instance,
		AllOfKeyword.Instance,
		AnyOfKeyword.Instance,
		ContainsKeyword.Instance,
		DependentSchemasKeyword.Instance,
		ElseKeyword.Instance,
		IfKeyword.Instance,
		ItemsKeyword.Instance,
		NotKeyword.Instance,
		OneOfKeyword.Instance,
		PatternPropertiesKeyword.Instance,
		PrefixItemsKeyword.Instance,
		PropertiesKeyword.Instance,
		PropertyNamesKeyword.Instance
	);

	public static Vocabulary Draft202012_Content { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
		ContentEncodingKeyword.Instance,
		ContentMediaTypeKeyword.Instance,
		ContentSchemaKeyword.Instance
	);

	public static Vocabulary Draft202012_Core { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/core"),
		AnchorKeyword.Instance,
		CommentKeyword.Instance,
		DefsKeyword.Instance,
		DynamicAnchorKeyword.Instance,
		Json.Schema.Keywords.Draft202012.DynamicRefKeyword.Instance,
		IdKeyword.Instance,
		RefKeyword.Instance,
		SchemaKeyword.Instance,
		Json.Schema.Keywords.Draft201909.VocabularyKeyword.Instance
	);

	public static Vocabulary Draft202012_FormatAnnotation { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
		Json.Schema.Keywords.Draft06.FormatKeyword.Annotate
	);

	public static Vocabulary Draft202012_FormatAssertion { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-assertion"),
		Json.Schema.Keywords.Draft06.FormatKeyword.Validate
	);

	public static Vocabulary Draft202012_MetaData { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
		DefaultKeyword.Instance,
		DeprecatedKeyword.Instance,
		DescriptionKeyword.Instance,
		ExamplesKeyword.Instance,
		ReadOnlyKeyword.Instance,
		TitleKeyword.Instance,
		WriteOnlyKeyword.Instance
	);

	public static Vocabulary Draft202012_Unevaluated { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"),
		UnevaluatedItemsKeyword.Instance,
		UnevaluatedPropertiesKeyword.Instance
	);

	public static Vocabulary Draft202012_Validation { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/validation"),
		ConstKeyword.Instance,
		DependentRequiredKeyword.Instance,
		EnumKeyword.Instance,
		ExclusiveMaximumKeyword.Instance,
		ExclusiveMinimumKeyword.Instance,
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
		PatternKeyword.Instance,
		RequiredKeyword.Instance,
		TypeKeyword.Instance,
		UniqueItemsKeyword.Instance
	);
}