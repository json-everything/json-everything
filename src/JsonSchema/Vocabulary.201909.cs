using System;
using Json.Schema.Keywords;
// ReSharper disable InconsistentNaming

namespace Json.Schema;

public partial class Vocabulary
{
	public static Vocabulary Draft201909_Applicator { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/applicator"),
		Json.Schema.Keywords.Draft06.AdditionalItemsKeyword.Instance,
		AdditionalPropertiesKeyword.Instance,
		AllOfKeyword.Instance,
		AnyOfKeyword.Instance,
		ContainsKeyword.Instance,
		DependentSchemasKeyword.Instance,
		ElseKeyword.Instance,
		IfKeyword.Instance,
		Json.Schema.Keywords.Draft06.ItemsKeyword.Instance,
		NotKeyword.Instance,
		OneOfKeyword.Instance,
		PatternPropertiesKeyword.Instance,
		PropertiesKeyword.Instance,
		PropertyNamesKeyword.Instance,
		ThenKeyword.Instance,
		Json.Schema.Keywords.Draft201909.UnevaluatedItemsKeyword.Instance,
		UnevaluatedPropertiesKeyword.Instance
	);

	public static Vocabulary Draft201909_Content { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/content"),
		ContentEncodingKeyword.Instance,
		ContentMediaTypeKeyword.Instance,
		ContentSchemaKeyword.Instance
	);

	public static Vocabulary Draft201909_Core { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/core"),
		Json.Schema.Keywords.Draft201909.AnchorKeyword.Instance,
		CommentKeyword.Instance,
		DefsKeyword.Instance,
		IdKeyword.Instance,
		Json.Schema.Keywords.Draft201909.RecursiveAnchorKeyword.Instance,
		Json.Schema.Keywords.Draft201909.RecursiveRefKeyword.Instance,
		RefKeyword.Instance,
		SchemaKeyword.Instance,
		Json.Schema.Keywords.Draft201909.VocabularyKeyword.Instance
	);

	public static Vocabulary Draft201909_Format { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/format"),
		Json.Schema.Keywords.Draft06.FormatKeyword.Annotate
	);

	public static Vocabulary Draft201909_MetaData { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/meta-data"),
		DefaultKeyword.Instance,
		DeprecatedKeyword.Instance,
		DescriptionKeyword.Instance,
		ExamplesKeyword.Instance,
		ReadOnlyKeyword.Instance,
		TitleKeyword.Instance,
		WriteOnlyKeyword.Instance
	);

	public static Vocabulary Draft201909_Validation { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/validation"),
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