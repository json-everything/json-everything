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

	/// <summary>
	/// Gets the Content vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/content'.</remarks>
	public static Vocabulary Draft202012_Content { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
		ContentEncodingKeyword.Instance,
		ContentMediaTypeKeyword.Instance,
		ContentSchemaKeyword.Instance
	);

	/// <summary>
	/// Gets the Core vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/core'.</remarks>
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

	/// <summary>
	/// Gets the Format-Annotation vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/format-annotation'.</remarks>
	public static Vocabulary Draft202012_FormatAnnotation { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
		Json.Schema.Keywords.Draft06.FormatKeyword.Annotate
	);

	/// <summary>
	/// Gets the Format-Assertion vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/format-assertion'.</remarks>
	public static Vocabulary Draft202012_FormatAssertion { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-assertion"),
		Json.Schema.Keywords.Draft06.FormatKeyword.Validate
	);

	/// <summary>
	/// Gets the Meta-Data vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/meta-data'.</remarks>
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

	/// <summary>
	/// Gets the Unevaluated vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/unevaluated'.</remarks>
	public static Vocabulary Draft202012_Unevaluated { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"),
		UnevaluatedItemsKeyword.Instance,
		UnevaluatedPropertiesKeyword.Instance
	);

	/// <summary>
	/// Gets the Validation vocabulary definition in JSON Schema Draft 2019-09.
	/// </summary>
	/// <remarks>The vocabulary is identified by the URI 'https://json-schema.org/draft/2019-09/vocab/validation'.</remarks>
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