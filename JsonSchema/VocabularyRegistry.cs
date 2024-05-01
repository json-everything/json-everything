using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Json.Schema;

/// <summary>
/// A registry for vocabularies.
/// </summary>
public static class VocabularyRegistry
{
	private static readonly ConcurrentDictionary<Uri, Vocabulary> _vocabularies = new();

	static VocabularyRegistry()
	{
		Register(Vocabularies.Core201909);
		Register(Vocabularies.Applicator201909);
		Register(Vocabularies.Validation201909);
		Register(Vocabularies.Metadata201909);
		Register(Vocabularies.Format201909);
		Register(Vocabularies.Content201909);

		Register(Vocabularies.Core202012);
		Register(Vocabularies.Applicator202012);
		Register(Vocabularies.Validation202012);
		Register(Vocabularies.Metadata202012);
		Register(Vocabularies.Unevaluated202012);
		Register(Vocabularies.FormatAnnotation202012);
		Register(Vocabularies.FormatAssertion202012);
		Register(Vocabularies.Content202012);

		Register(Vocabularies.CoreNext);
		Register(Vocabularies.ApplicatorNext);
		Register(Vocabularies.ValidationNext);
		Register(Vocabularies.MetadataNext);
		Register(Vocabularies.UnevaluatedNext);
		Register(Vocabularies.FormatAnnotationNext);
		Register(Vocabularies.FormatAssertionNext);
		Register(Vocabularies.ContentNext);
	}

	/// <summary>
	/// Registers a vocabulary.  This does not register the vocabulary's
	/// keywords.  This must be done separately.
	/// </summary>
	/// <param name="vocabulary"></param>
	public static void Register(Vocabulary vocabulary)
	{
		_vocabularies[vocabulary.Id] = vocabulary;
	}

	/// <summary>
	/// Removes a vocabulary from the registry.
	/// </summary>
	/// <param name="vocabulary"></param>
	public static void Unregister(Vocabulary vocabulary)
	{
		_vocabularies.TryRemove(vocabulary.Id, out _);
	}

	/// <summary>
	/// Indicates whether a vocabulary is known by URI ID and/or anchor.
	/// </summary>
	/// <param name="vocabularyId">The URI ID.</param>
	/// <returns>
	/// `true`, if registered in either this or the global registry;
	/// `false` otherwise.
	/// </returns>
	public static bool IsKnown(Uri vocabularyId)
	{
		return _vocabularies.ContainsKey(vocabularyId);
	}

	/// <summary>
	/// Retrieves the vocabulary associated with the URI ID, if known.
	/// </summary>
	/// <param name="vocabularyId">The URI ID.</param>
	/// <returns>The vocabulary, if known; otherwise null.</returns>
	public static Vocabulary? Get(Uri vocabularyId)
	{
		return _vocabularies.GetValueOrDefault(vocabularyId);
	}
}