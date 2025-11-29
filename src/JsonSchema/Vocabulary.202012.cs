using System;
using Json.Schema.Keywords;

namespace Json.Schema;

public partial class Vocabulary
{
	public static Vocabulary Draft202012_Applicator { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"),
		new AdditionalPropertiesKeyword(),
		new AllOfKeyword(),
		new AnyOfKeyword(),
		new ContainsKeyword(),
		new DependentSchemasKeyword(),
		new ElseKeyword(),
		new IfKeyword(),
		new ItemsKeyword(),
		new NotKeyword(),
		new OneOfKeyword(),
		new PatternPropertiesKeyword(),
		new PrefixItemsKeyword(),
		new PropertiesKeyword(),
		new PropertyNamesKeyword()
	);

	public static Vocabulary Draft202012_Content { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
		new ContentEncodingKeyword(),
		new ContentMediaTypeKeyword(),
		new ContentSchemaKeyword()
	);

	public static Vocabulary Draft202012_Core { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/core"),
		new AnchorKeyword(),
		new CommentKeyword(),
		new DefsKeyword(),
		new DynamicAnchorKeyword(),
		new Keywords.Draft202012.DynamicRefKeyword(),
		new IdKeyword(),
		new RefKeyword(),
		new SchemaKeyword(),
		new VocabularyKeyword()
	);

	public static Vocabulary Draft202012_FormatAnnotation { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
		new Keywords.Draft06.FormatKeyword()
	);

	public static Vocabulary Draft202012_FormatAssertion { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-assertion"),
		new Keywords.Draft06.FormatKeyword{RequireFormatValidation = true}
	);

	public static Vocabulary Draft202012_MetaData { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
		new DefaultKeyword(),
		new DeprecatedKeyword(),
		new DescriptionKeyword(),
		new ExamplesKeyword(),
		new ReadOnlyKeyword(),
		new TitleKeyword(),
		new WriteOnlyKeyword()
	);

	public static Vocabulary Draft202012_Unevaluated { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"),
		new UnevaluatedItemsKeyword(),
		new UnevaluatedPropertiesKeyword()
	);

	public static Vocabulary Draft202012_Validation { get; } = new(
		new Uri("https://json-schema.org/draft/2020-12/vocab/validation"),
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