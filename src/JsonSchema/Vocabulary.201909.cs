using System;
using Json.Schema.Keywords;
// ReSharper disable InconsistentNaming

namespace Json.Schema;

public partial class Vocabulary
{
	/// <summary>
	/// Gets the Applicator vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/applicator'.</remarks>
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

	/// <summary>
	/// Gets the Content vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/content'.</remarks>
	public static Vocabulary Draft201909_Content { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/content"),
		ContentEncodingKeyword.Instance,
		ContentMediaTypeKeyword.Instance,
		ContentSchemaKeyword.Instance
	);

	/// <summary>
	/// Gets the Core vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/core'.</remarks>
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

	/// <summary>
	/// Gets the Format vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/format'.</remarks>
	public static Vocabulary Draft201909_Format { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/format"),
		Json.Schema.Keywords.Draft06.FormatKeyword.Annotate
	);

	/// <summary>
	/// Gets the Meta-Data vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/meta-data'.</remarks>
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

	/// <summary>
	/// Gets the Validation vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/validation'.</remarks>
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