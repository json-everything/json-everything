using System;
using System.Collections.Concurrent;

namespace Json.Schema;

/// <summary>
/// A registry for vocabularies.
/// </summary>
public class VocabularyRegistry
{
	private ConcurrentDictionary<Uri, Vocabulary>? _vocabularies;

	/// <summary>
	/// The global registry.
	/// </summary>
	public static VocabularyRegistry Global { get; } = new();

	/// <summary>
	/// Registers a vocabulary.  This does not register the vocabulary's
	/// keywords.  This must be done separately.
	/// </summary>
	/// <param name="vocabulary"></param>
	public void Register(Vocabulary vocabulary)
	{
		_vocabularies ??= new ConcurrentDictionary<Uri, Vocabulary>();
		_vocabularies[vocabulary.Id] = vocabulary;
	}

	/// <summary>
	/// Indicates whether a vocabulary is known by URI ID and/or anchor.
	/// </summary>
	/// <param name="vocabularyId">The URI ID.</param>
	/// <returns>
	/// `true`, if registered in either this or the global registry;
	/// `false` otherwise.
	/// </returns>
	public bool IsKnown(Uri vocabularyId)
	{
		if (_vocabularies != null && _vocabularies.ContainsKey(vocabularyId)) return true;

		if (!ReferenceEquals(this, Global))
			return Global.IsKnown(vocabularyId);

		return false;
	}

	/// <summary>
	/// Retrieves the vocabulary associated with the URI ID, if known.
	/// </summary>
	/// <param name="vocabularyId">The URI ID.</param>
	/// <returns>The vocabulary, if known; otherwise null.</returns>
	public Vocabulary? Get(Uri vocabularyId)
	{
		if (_vocabularies != null && _vocabularies.TryGetValue(vocabularyId, out var vocabulary)) return vocabulary;

		if (!ReferenceEquals(this, Global))
			return Global.Get(vocabularyId);

		return null;
	}

	internal void CopyFrom(VocabularyRegistry other)
	{
		if (other._vocabularies == null) return;

		if (_vocabularies == null)
		{
			_vocabularies = new ConcurrentDictionary<Uri, Vocabulary>(other._vocabularies);
			return;
		}

		foreach (var vocabulary in other._vocabularies)
		{
			_vocabularies[vocabulary.Key] = vocabulary.Value;
		}
	}
}