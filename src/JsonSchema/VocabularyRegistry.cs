using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Provides a registry for managing JSON Schema vocabularies, allowing registration and unregistration of custom
/// vocabularies in addition to well-known official vocabularies.
/// </summary>
/// <remarks>The registry maintains a set of official vocabularies that cannot be modified or removed. Use the
/// <see cref="Global"/> property to access a shared, application-wide registry instance. Getting vocabularies via
/// local instances fall back to the global registry.</remarks>
public class VocabularyRegistry
{
	private readonly Dictionary<Uri, Vocabulary> _vocabs = new();
	private Uri[] _wellKnownVocabs = null!;

	public static VocabularyRegistry Global { get; } = new();

	public void Register(Vocabulary vocabulary)
	{
		if (_wellKnownVocabs.Contains(vocabulary.Id))
			throw new ArgumentException("Cannot overwrite official vocabularies");

		_vocabs[vocabulary.Id] = vocabulary;
	}

	public void Unregister(Uri vocabularyId)
	{
		if (_wellKnownVocabs.Contains(vocabularyId))
			throw new ArgumentException("Cannot remove official vocabularies");

		_vocabs.Remove(vocabularyId);
	}

	internal Vocabulary? GetVocab(Uri vocabUri) =>
		_vocabs.GetValueOrDefault(vocabUri) ??
		Global._vocabs.GetValueOrDefault(vocabUri);

	internal void RegisterDefaultVocabs()
	{
		_vocabs[new Uri("https://json-schema.org/draft/2019-09/vocab/core")] = Vocabulary.Draft201909_Core;
		_vocabs[new Uri("https://json-schema.org/draft/2019-09/vocab/applicator")] = Vocabulary.Draft201909_Applicator;
		_vocabs[new Uri("https://json-schema.org/draft/2019-09/vocab/validation")] = Vocabulary.Draft201909_Validation;
		_vocabs[new Uri("https://json-schema.org/draft/2019-09/vocab/meta-data")] = Vocabulary.Draft201909_MetaData;
		_vocabs[new Uri("https://json-schema.org/draft/2019-09/vocab/format")] = Vocabulary.Draft201909_Format;
		_vocabs[new Uri("https://json-schema.org/draft/2019-09/vocab/content")] = Vocabulary.Draft201909_Content;
		
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/core")] = Vocabulary.Draft202012_Core;
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/applicator")] = Vocabulary.Draft202012_Applicator;
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/validation")] = Vocabulary.Draft202012_Validation;
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data")] = Vocabulary.Draft202012_MetaData;
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation")] = Vocabulary.Draft202012_FormatAnnotation;
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/format-assertion")] = Vocabulary.Draft202012_FormatAssertion;
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/content")] = Vocabulary.Draft202012_Content;
		_vocabs[new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated")] = Vocabulary.Draft202012_Unevaluated;

		_wellKnownVocabs = _vocabs.Keys.ToArray();
	}
}