using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static partial class Vocabularies
{
	static Vocabularies()
	{
		var keywords = new (Type, IEnumerable<VocabularyAttribute>)[] {
			( typeof(AdditionalItemsKeyword), typeof(AdditionalItemsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(AdditionalPropertiesKeyword), typeof(AdditionalPropertiesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(AllOfKeyword), typeof(AllOfKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(AnchorKeyword), typeof(AnchorKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(AnyOfKeyword), typeof(AnyOfKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(CommentKeyword), typeof(CommentKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ConstKeyword), typeof(ConstKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ContainsKeyword), typeof(ContainsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ContentEncodingKeyword), typeof(ContentEncodingKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ContentMediaTypeKeyword), typeof(ContentMediaTypeKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ContentSchemaKeyword), typeof(ContentSchemaKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DefaultKeyword), typeof(DefaultKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DefinitionsKeyword), typeof(DefinitionsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DefsKeyword), typeof(DefsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DependenciesKeyword), typeof(DependenciesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DependentRequiredKeyword), typeof(DependentRequiredKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DependentSchemasKeyword), typeof(DependentSchemasKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DeprecatedKeyword), typeof(DeprecatedKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DescriptionKeyword), typeof(DescriptionKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DynamicAnchorKeyword), typeof(DynamicAnchorKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(DynamicRefKeyword), typeof(DynamicRefKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ElseKeyword), typeof(ElseKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(EnumKeyword), typeof(EnumKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ExamplesKeyword), typeof(ExamplesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ExclusiveMaximumKeyword), typeof(ExclusiveMaximumKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ExclusiveMinimumKeyword), typeof(ExclusiveMinimumKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(FormatKeyword), typeof(FormatKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(IdKeyword), typeof(IdKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(IfKeyword), typeof(IfKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ItemsKeyword), typeof(ItemsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MaxContainsKeyword), typeof(MaxContainsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MaximumKeyword), typeof(MaximumKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MaxItemsKeyword), typeof(MaxItemsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MaxLengthKeyword), typeof(MaxLengthKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MaxPropertiesKeyword), typeof(MaxPropertiesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MinContainsKeyword), typeof(MinContainsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MinimumKeyword), typeof(MinimumKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MinItemsKeyword), typeof(MinItemsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MinLengthKeyword), typeof(MinLengthKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MinPropertiesKeyword), typeof(MinPropertiesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(MultipleOfKeyword), typeof(MultipleOfKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(NotKeyword), typeof(NotKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(OneOfKeyword), typeof(OneOfKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(PatternKeyword), typeof(PatternKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(PatternPropertiesKeyword), typeof(PatternPropertiesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(PrefixItemsKeyword), typeof(PrefixItemsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(PropertiesKeyword), typeof(PropertiesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(PropertyDependenciesKeyword), typeof(PropertyDependenciesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(PropertyNamesKeyword), typeof(PropertyNamesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ReadOnlyKeyword), typeof(ReadOnlyKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(RecursiveAnchorKeyword), typeof(RecursiveAnchorKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(RecursiveRefKeyword), typeof(RecursiveRefKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(RefKeyword), typeof(RefKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(RequiredKeyword), typeof(RequiredKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(SchemaKeyword), typeof(SchemaKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(ThenKeyword), typeof(ThenKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(TitleKeyword), typeof(TitleKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(TypeKeyword), typeof(TypeKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(UnevaluatedItemsKeyword), typeof(UnevaluatedItemsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(UnevaluatedPropertiesKeyword), typeof(UnevaluatedPropertiesKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(UniqueItemsKeyword), typeof(UniqueItemsKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(VocabularyKeyword), typeof(VocabularyKeyword).GetCustomAttributes<VocabularyAttribute>() ),
			( typeof(WriteOnlyKeyword), typeof(WriteOnlyKeyword).GetCustomAttributes<VocabularyAttribute>() ),      
		};

		Core201909 = new Vocabulary(
			Core201909Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Core201909Id))
				.Select(k => k.Item1));
		Applicator201909 = new Vocabulary(
			Applicator201909Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Applicator201909Id))
				.Select(k => k.Item1));
		Validation201909 = new Vocabulary(
			Validation201909Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Validation201909Id))
				.Select(k => k.Item1));
		Metadata201909 = new Vocabulary(
			Metadata201909Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Metadata201909Id))
				.Select(k => k.Item1));
		Format201909 = new Vocabulary(
			Format201909Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Format201909Id))
				.Select(k => k.Item1));
		Content201909 = new Vocabulary(
			Content201909Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Content201909Id))
				.Select(k => k.Item1));

		Core202012 = new Vocabulary(
			Core202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Core202012Id))
				.Select(k => k.Item1));
		Unevaluated202012 = new Vocabulary(
			Unevaluated202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Unevaluated202012Id))
				.Select(k => k.Item1));
		Applicator202012 = new Vocabulary(
			Applicator202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Applicator202012Id))
				.Select(k => k.Item1));
		Validation202012 = new Vocabulary(
			Validation202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Validation202012Id))
				.Select(k => k.Item1));
		Metadata202012 = new Vocabulary(
			Metadata202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Metadata202012Id))
				.Select(k => k.Item1));
		FormatAnnotation202012 = new Vocabulary(
			FormatAnnotation202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == FormatAnnotation202012Id))
				.Select(k => k.Item1));
		FormatAssertion202012 = new Vocabulary(
			FormatAssertion202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == FormatAssertion202012Id))
				.Select(k => k.Item1));
		Content202012 = new Vocabulary(
			Content202012Id,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == Content202012Id))
				.Select(k => k.Item1));

		CoreNext = new Vocabulary(
			CoreNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == CoreNextId))
				.Select(k => k.Item1));
		UnevaluatedNext = new Vocabulary(
			UnevaluatedNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == UnevaluatedNextId))
				.Select(k => k.Item1));
		ApplicatorNext = new Vocabulary(
			ApplicatorNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == ApplicatorNextId))
				.Select(k => k.Item1));
		ValidationNext = new Vocabulary(
			ValidationNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == ValidationNextId))
				.Select(k => k.Item1));
		MetadataNext = new Vocabulary(
			MetadataNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == MetadataNextId))
				.Select(k => k.Item1));
		FormatAnnotationNext = new Vocabulary(
			FormatAnnotationNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == FormatAnnotationNextId))
				.Select(k => k.Item1));
		FormatAssertionNext = new Vocabulary(
			FormatAssertionNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == FormatAssertionNextId))
				.Select(k => k.Item1));
		ContentNext = new Vocabulary(
			ContentNextId,
			keywords.Where(k => k.Item2.Any(v => v.Id.OriginalString == ContentNextId))
				.Select(k => k.Item1));
	}
}