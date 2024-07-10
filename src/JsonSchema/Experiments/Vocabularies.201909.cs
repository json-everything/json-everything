using System;

namespace Json.Schema.Experiments;

public static partial class Vocabularies
{
	public static readonly Vocabulary Core201909 =
		new(new Uri("https://json-schema.org/draft/2019-09/vocab/core"),
			MetaSchemas.Core201909,
			IdKeywordHandler.Instance,
			SchemaKeywordHandler.Instance,
			RefKeywordHandler.Instance,
			AnchorKeywordHandler.Instance,
			RecursiveRefKeywordHandler.Instance,
			RecursiveAnchorKeywordHandler.Instance,
			VocabularyKeywordHandler.Instance,
			CommentKeywordHandler.Instance,
			DefsKeywordHandler.Instance);

	public static readonly Vocabulary Applicator201909 =
		new(new Uri("https://json-schema.org/draft/2019-09/vocab/applicator"),
			MetaSchemas.Applicator201909,
			ItemsKeywordHandler.AllowArrays,
			AdditionalItemsKeywordHandler.Instance,
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
			NotKeywordHandler.Instance,
			UnevaluatedItemsKeywordHandler.Instance,
			UnevaluatedPropertiesKeywordHandler.Instance);

	public static readonly Vocabulary Validation201909 =
		new(new Uri("https://json-schema.org/draft/2019-09/vocab/validation"),
			MetaSchemas.Validation201909,
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

	public static readonly Vocabulary MetaData201909 =
		new(new Uri("https://json-schema.org/draft/2019-09/vocab/meta-data"),
			MetaSchemas.Metadata201909,
			TitleKeywordHandler.Instance,
			DescriptionKeywordHandler.Instance,
			DefaultKeywordHandler.Instance,
			DeprecatedKeywordHandler.Instance,
			ReadOnlyKeywordHandler.Instance,
			WriteOnlyKeywordHandler.Instance,
			ExamplesKeywordHandler.Instance);

	public static readonly Vocabulary Format201909 =
		new(new Uri("https://json-schema.org/draft/2019-09/vocab/format"),
			MetaSchemas.Format201909,
			FormatKeywordHandler.Annotate);

	public static readonly Vocabulary Content201909 =
		new(new Uri("https://json-schema.org/draft/2019-09/vocab/content"),
			MetaSchemas.Content201909,
			ContentEncodingKeywordHandler.Instance,
			ContentMediaTypeKeywordHandler.Instance,
			ContentSchemaKeywordHandler.Instance);
}