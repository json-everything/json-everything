using System;

namespace Json.Schema.Experiments;

public static partial class Vocabularies
{
	public static readonly Vocabulary Core202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/core"),
			MetaSchemas.Core202012,
			IdKeywordHandler.Instance,
			SchemaKeywordHandler.Instance,
			RefKeywordHandler.Instance,
			AnchorKeywordHandler.Instance,
			DynamicRefKeywordHandler.RequireAdjacentAnchor,
			DynamicAnchorKeywordHandler.Instance,
			VocabularyKeywordHandler.Instance,
			CommentKeywordHandler.Instance,
			DefsKeywordHandler.Instance);

	public static readonly Vocabulary Applicator202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"),
			MetaSchemas.Applicator202012,
			PrefixItemsKeywordHandler.Instance,
			ItemsKeywordHandler.OnlySingle,
			ContainsKeywordHandler.Instance,
			AdditionalPropertiesKeywordHandler.Instance,
			PropertiesKeywordHandler.Instance,
			PatternPropertiesKeywordHandler.Instance,
			DependentSchemasKeywordHandler.Instance,
			PropertyNamesKeywordHandler.Instance,
			IfKeywordHandler.Instance,
			ThenKeywordHandler.Instance,
			ElseKeywordHandler.Instance,
			AllOfKeywordHandler.Instance,
			AnyOfKeywordHandler.Instance,
			OneOfKeywordHandler.Instance,
			NotKeywordHandler.Instance);

	public static readonly Vocabulary Unevaluated202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"),
			MetaSchemas.Unevaluated202012,
			UnevaluatedItemsKeywordHandler.Instance,
			UnevaluatedPropertiesKeywordHandler.Instance);

	public static readonly Vocabulary Validation202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/validation"),
			MetaSchemas.Validation202012,
			TypeKeywordHandler.Instance,
			ConstKeywordHandler.Instance,
			EnumKeywordHandler.Instance,
			MultipleOfKeywordHandler.Instance,
			MaximumKeywordHandler.Instance,
			ExclusiveMaximumKeywordHandler.Instance,
			MinimumKeywordHandler.Instance,
			ExclusiveMinimumKeywordHandler.Instance,
			MaxLengthKeywordHandler.Instance,
			MinLengthKeywordHandler.Instance,
			PatternKeywordHandler.Instance,
			MaxItemsKeywordHandler.Instance,
			MinItemsKeywordHandler.Instance,
			UniqueItemsKeywordHandler.Instance,
			MaxContainsKeywordHandler.Instance,
			MinContainsKeywordHandler.Instance,
			MaxPropertiesKeywordHandler.Instance,
			MinPropertiesKeywordHandler.Instance,
			RequiredKeywordHandler.Instance,
			DependentRequiredKeywordHandler.Instance);

	public static readonly Vocabulary MetaData202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
			MetaSchemas.Metadata202012,
			TitleKeywordHandler.Instance,
			DescriptionKeywordHandler.Instance,
			DefaultKeywordHandler.Instance,
			DeprecatedKeywordHandler.Instance,
			ReadOnlyKeywordHandler.Instance,
			WriteOnlyKeywordHandler.Instance,
			ExamplesKeywordHandler.Instance);

	public static readonly Vocabulary FormatAnnotation202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
			MetaSchemas.FormatAnnotation202012,
			FormatKeywordHandler.Annotate);

	public static readonly Vocabulary FormatAssertion202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/format-assertion"),
			MetaSchemas.FormatAssertion202012,
			FormatKeywordHandler.Assert);

	public static readonly Vocabulary Content202012 =
		new(new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
			MetaSchemas.Content202012,
			ContentEncodingKeywordHandler.Instance,
			ContentMediaTypeKeywordHandler.Instance,
			ContentSchemaKeywordHandler.Instance);
}