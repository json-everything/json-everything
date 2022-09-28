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
		var keywords = typeof(IJsonSchemaKeyword)
			.Assembly
			.DefinedTypes
			.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
						!t.IsAbstract &&
						!t.IsInterface)
			.Select(t => new
			{
				Type = t,
				Vocabularies = t.GetCustomAttributes<VocabularyAttribute>()
			})
			.ToList();

		Core201909 = new Vocabulary(
			Core201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Core201909Id))
				.Select(k => k.Type));
		Applicator201909 = new Vocabulary(
			Applicator201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Applicator201909Id))
				.Select(k => k.Type));
		Validation201909 = new Vocabulary(
			Validation201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Validation201909Id))
				.Select(k => k.Type));
		Metadata201909 = new Vocabulary(
			Metadata201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Metadata201909Id))
				.Select(k => k.Type));
		Format201909 = new Vocabulary(
			Format201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Format201909Id))
				.Select(k => k.Type));
		Content201909 = new Vocabulary(
			Content201909Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Content201909Id))
				.Select(k => k.Type));

		Core202012 = new Vocabulary(
			Core202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Core202012Id))
				.Select(k => k.Type));
		Unevaluated202012 = new Vocabulary(
			Unevaluated202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Unevaluated202012Id))
				.Select(k => k.Type));
		Applicator202012 = new Vocabulary(
			Applicator202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Applicator202012Id))
				.Select(k => k.Type));
		Validation202012 = new Vocabulary(
			Validation202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Validation202012Id))
				.Select(k => k.Type));
		Metadata202012 = new Vocabulary(
			Metadata202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Metadata202012Id))
				.Select(k => k.Type));
		FormatAnnotation202012 = new Vocabulary(
			FormatAnnotation202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAnnotation202012Id))
				.Select(k => k.Type));
		FormatAssertion202012 = new Vocabulary(
			FormatAssertion202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAssertion202012Id))
				.Select(k => k.Type));
		Content202012 = new Vocabulary(
			Content202012Id,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == Content202012Id))
				.Select(k => k.Type));

		CoreNext = new Vocabulary(
			CoreNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == CoreNextId))
				.Select(k => k.Type));
		UnevaluatedNext = new Vocabulary(
			UnevaluatedNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == UnevaluatedNextId))
				.Select(k => k.Type));
		ApplicatorNext = new Vocabulary(
			ApplicatorNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == ApplicatorNextId))
				.Select(k => k.Type));
		ValidationNext = new Vocabulary(
			ValidationNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == ValidationNextId))
				.Select(k => k.Type));
		MetadataNext = new Vocabulary(
			MetadataNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == MetadataNextId))
				.Select(k => k.Type));
		FormatAnnotationNext = new Vocabulary(
			FormatAnnotationNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAnnotationNextId))
				.Select(k => k.Type));
		FormatAssertionNext = new Vocabulary(
			FormatAssertionNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == FormatAssertionNextId))
				.Select(k => k.Type));
		ContentNext = new Vocabulary(
			ContentNextId,
			keywords.Where(k => k.Vocabularies.Any(v => v.Id.OriginalString == ContentNextId))
				.Select(k => k.Type));
	}
}