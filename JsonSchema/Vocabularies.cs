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
		var keywords = SchemaKeywordRegistry.KeywordTypes
			.Select(t => new
			{
				Type = t,
				Vocabularies = t.GetCustomAttributes<VocabularyAttribute>()
			})
			.ToArray();

		Core201909 = new Vocabulary(
			Core201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Core201909Id))
				.Select(k => k.Type),
			MetaSchemas.Core201909);
		Applicator201909 = new Vocabulary(
			Applicator201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Applicator201909Id))
				.Select(k => k.Type),
			MetaSchemas.Applicator201909);
		Validation201909 = new Vocabulary(
			Validation201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Validation201909Id))
				.Select(k => k.Type),
			MetaSchemas.Validation201909);
		Metadata201909 = new Vocabulary(
			Metadata201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Metadata201909Id))
				.Select(k => k.Type),
			MetaSchemas.Metadata201909);
		Format201909 = new Vocabulary(
			Format201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Format201909Id))
				.Select(k => k.Type),
			MetaSchemas.Format201909);
		Content201909 = new Vocabulary(
			Content201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Content201909Id))
				.Select(k => k.Type),
			MetaSchemas.Content201909);

		Core202012 = new Vocabulary(
			Core202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Core202012Id))
				.Select(k => k.Type),
			MetaSchemas.Core202012);
		Unevaluated202012 = new Vocabulary(
			Unevaluated202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Unevaluated202012Id))
				.Select(k => k.Type),
			MetaSchemas.Unevaluated202012);
		Applicator202012 = new Vocabulary(
			Applicator202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Applicator202012Id))
				.Select(k => k.Type),
			MetaSchemas.Applicator202012);
		Validation202012 = new Vocabulary(
			Validation202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Validation202012Id))
				.Select(k => k.Type),
			MetaSchemas.Validation202012);
		Metadata202012 = new Vocabulary(
			Metadata202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Metadata202012Id))
				.Select(k => k.Type),
			MetaSchemas.Metadata202012);
		FormatAnnotation202012 = new Vocabulary(
			FormatAnnotation202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAnnotation202012Id))
				.Select(k => k.Type),
			MetaSchemas.FormatAnnotation202012);
		FormatAssertion202012 = new Vocabulary(
			FormatAssertion202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAssertion202012Id))
				.Select(k => k.Type),
			MetaSchemas.FormatAssertion202012);
		Content202012 = new Vocabulary(
			Content202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Content202012Id))
				.Select(k => k.Type),
			MetaSchemas.Content202012);

		CoreNext = new Vocabulary(
			CoreNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == CoreNextId))
				.Select(k => k.Type),
			MetaSchemas.CoreNext);
		UnevaluatedNext = new Vocabulary(
			UnevaluatedNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == UnevaluatedNextId))
				.Select(k => k.Type),
			MetaSchemas.UnevaluatedNext);
		ApplicatorNext = new Vocabulary(
			ApplicatorNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == ApplicatorNextId))
				.Select(k => k.Type),
			MetaSchemas.ApplicatorNext);
		ValidationNext = new Vocabulary(
			ValidationNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == ValidationNextId))
				.Select(k => k.Type),
			MetaSchemas.ValidationNext);
		MetadataNext = new Vocabulary(
			MetadataNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == MetadataNextId))
				.Select(k => k.Type),
			MetaSchemas.MetadataNext);
		FormatAnnotationNext = new Vocabulary(
			FormatAnnotationNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAnnotationNextId))
				.Select(k => k.Type),
			MetaSchemas.FormatAnnotationNext);
		FormatAssertionNext = new Vocabulary(
			FormatAssertionNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAssertionNextId))
				.Select(k => k.Type),
			MetaSchemas.FormatAssertionNext);
		ContentNext = new Vocabulary(
			ContentNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == ContentNextId))
				.Select(k => k.Type),
			MetaSchemas.ContentNext);

		VocabularyRegistry.Global.Register(Core201909);
		VocabularyRegistry.Global.Register(Applicator201909);
		VocabularyRegistry.Global.Register(Validation201909);
		VocabularyRegistry.Global.Register(Metadata201909);
		VocabularyRegistry.Global.Register(Format201909);
		VocabularyRegistry.Global.Register(Content201909);
		
		VocabularyRegistry.Global.Register(Core202012);
		VocabularyRegistry.Global.Register(Applicator202012);
		VocabularyRegistry.Global.Register(Validation202012);
		VocabularyRegistry.Global.Register(Metadata202012);
		VocabularyRegistry.Global.Register(Unevaluated202012);
		VocabularyRegistry.Global.Register(FormatAnnotation202012);
		VocabularyRegistry.Global.Register(FormatAssertion202012);
		VocabularyRegistry.Global.Register(Content202012);
		
		VocabularyRegistry.Global.Register(CoreNext);
		VocabularyRegistry.Global.Register(ApplicatorNext);
		VocabularyRegistry.Global.Register(ValidationNext);
		VocabularyRegistry.Global.Register(MetadataNext);
		VocabularyRegistry.Global.Register(UnevaluatedNext);
		VocabularyRegistry.Global.Register(FormatAnnotationNext);
		VocabularyRegistry.Global.Register(FormatAssertionNext);
		VocabularyRegistry.Global.Register(ContentNext);

	}
}