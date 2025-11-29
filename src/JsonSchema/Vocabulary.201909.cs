using System;
using Json.Schema.Keywords;

namespace Json.Schema;

public partial class Vocabulary
{
	public static Vocabulary Draft201909_Applicator { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/applicator"),
		new Keywords.Draft06.AdditionalItemsKeyword(),
		new AdditionalPropertiesKeyword(),
		new AllOfKeyword(),
		new AnyOfKeyword(),
		new ContainsKeyword(),
		new DependentSchemasKeyword(),
		new ElseKeyword(),
		new IfKeyword(),
		new Keywords.Draft06.ItemsKeyword(),
		new NotKeyword(),
		new OneOfKeyword(),
		new PatternPropertiesKeyword(),
		new PropertiesKeyword(),
		new PropertyNamesKeyword(),
		new ThenKeyword(),
		new Keywords.Draft201909.UnevaluatedItemsKeyword(),
		new UnevaluatedPropertiesKeyword()
	);

	public static Vocabulary Draft201909_Content { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/content"),
		new ContentEncodingKeyword(),
		new ContentMediaTypeKeyword(),
		new ContentSchemaKeyword()
	);

	public static Vocabulary Draft201909_Core { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/core"),
		new Keywords.Draft201909.AnchorKeyword(),
		new CommentKeyword(),
		new DefsKeyword(),
		new IdKeyword(),
		new Keywords.Draft201909.RecursiveAnchorKeyword(),
		new Keywords.Draft201909.RecursiveRefKeyword(),
		new RefKeyword(),
		new SchemaKeyword(),
		new VocabularyKeyword()
	);

	public static Vocabulary Draft201909_Format { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/format"),
		new Keywords.Draft06.FormatKeyword()
	);

	public static Vocabulary Draft201909_MetaData { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/meta-data"),
		new DefaultKeyword(),
		new DeprecatedKeyword(),
		new DescriptionKeyword(),
		new ExamplesKeyword(),
		new ReadOnlyKeyword(),
		new TitleKeyword(),
		new WriteOnlyKeyword()
	);

	public static Vocabulary Draft201909_Validation { get; } = new(
		new Uri("https://json-schema.org/draft/2019-09/vocab/validation"),
		new ConstKeyword(),
		new DependentRequiredKeyword(),
		new EnumKeyword(),
		new ExclusiveMaximumKeyword(),
		new ExclusiveMinimumKeyword(),
		new MaxContainsKeyword(),
		new MaximumKeyword(),
		new MaxItemsKeyword(),
		new MaxLengthKeyword(),
		new MaxPropertiesKeyword(),
		new MinContainsKeyword(),
		new MinimumKeyword(),
		new MinItemsKeyword(),
		new MinLengthKeyword(),
		new MinPropertiesKeyword(),
		new MultipleOfKeyword(),
		new PatternKeyword(),
		new RequiredKeyword(),
		new TypeKeyword(),
		new UniqueItemsKeyword()
	);
}