using System;
using System.Collections.Concurrent;

namespace Json.Schema
{
	/// <summary>
	/// A registry for vocabularies.
	/// </summary>
	public class VocabularyRegistry
	{
		private ConcurrentDictionary<Uri, Vocabulary>? _vocabularies;

		/// <summary>
		/// The global registry.
		/// </summary>
		public static VocabularyRegistry Global { get; }

		static VocabularyRegistry()
		{
			Global = new VocabularyRegistry();

			Global.Register(Vocabularies.Core201909);
			Global.Register(Vocabularies.Applicator201909);
			Global.Register(Vocabularies.Validation201909);
			Global.Register(Vocabularies.Metadata201909);
			Global.Register(Vocabularies.Format201909);
			Global.Register(Vocabularies.Content201909);

			Global.Register(Vocabularies.Core202012);
			Global.Register(Vocabularies.Applicator202012);
			Global.Register(Vocabularies.Validation202012);
			Global.Register(Vocabularies.Metadata202012);
			Global.Register(Vocabularies.Unevaluated202012);
			Global.Register(Vocabularies.FormatAnnotation202012);
			Global.Register(Vocabularies.FormatAssertion202012);
			Global.Register(Vocabularies.Content202012);
		}

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
		/// <code>true</code>, if registered in either this or the global registry;
		/// <code>false</code> otherwise.
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
}