using System;

namespace Json.Schema.Experiments;

public static partial class Vocabularies
{
	public static readonly Vocabulary CoreNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/core"),
			MetaSchemas.CoreNext,
			IdKeywordHandler.Instance,
			SchemaKeywordHandler.Instance,
			RefKeywordHandler.Instance,
			AnchorKeywordHandler.Instance,
			DynamicRefKeywordHandler.Instance,
			DynamicAnchorKeywordHandler.Instance,
			VocabularyKeywordHandler.Instance,
			CommentKeywordHandler.Instance,
			DefsKeywordHandler.Instance);

	public static readonly Vocabulary ApplicatorNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/applicator"),
			MetaSchemas.ApplicatorNext,
			PrefixItemsKeywordHandler.Instance,
			ItemsKeywordHandler.AllowArrays,
			ContainsKeywordHandler.Instance,
			AdditionalPropertiesKeywordHandler.Instance,
			PropertiesKeywordHandler.Instance,
			PatternPropertiesKeywordHandler.Instance,
			DependentSchemasKeywordHandler.Instance,
			PropertyNamesKeywordHandler.Instance,
			PropertyDependenciesKeywordHandler.Instance,
			IfKeywordHandler.Instance,
			ThenKeywordHandler.Instance,
			ElseKeywordHandler.Instance,
			AllOfKeywordHandler.Instance,
			AnyOfKeywordHandler.Instance,
			OneOfKeywordHandler.Instance,
			NotKeywordHandler.Instance);

	public static readonly Vocabulary UnevaluatedNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/unevaluated"),
			MetaSchemas.UnevaluatedNext,
			UnevaluatedItemsKeywordHandler.Instance,
			UnevaluatedPropertiesKeywordHandler.Instance);

	public static readonly Vocabulary ValidationNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/validation"),
			MetaSchemas.ValidationNext,
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

	public static readonly Vocabulary MetaDataNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/meta-data"),
			MetaSchemas.MetadataNext,
			TitleKeywordHandler.Instance,
			DescriptionKeywordHandler.Instance,
			DefaultKeywordHandler.Instance,
			DeprecatedKeywordHandler.Instance,
			ReadOnlyKeywordHandler.Instance,
			WriteOnlyKeywordHandler.Instance,
			ExamplesKeywordHandler.Instance);

	public static readonly Vocabulary FormatAnnotationNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/format-annotation"),
			MetaSchemas.FormatAnnotationNext,
			FormatKeywordHandler.Annotate);

	public static readonly Vocabulary FormatAssertionNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/format-assertion"),
			MetaSchemas.FormatAssertionNext,
			FormatKeywordHandler.Assert);

	public static readonly Vocabulary ContentNext =
		new(new Uri("https://json-schema.org/draft/next/vocab/content"),
			MetaSchemas.ContentNext,
			ContentEncodingKeywordHandler.Instance,
			ContentMediaTypeKeywordHandler.Instance,
			ContentSchemaKeywordHandler.Instance);
}