using System;
using System.Collections.Concurrent;

namespace Json.Schema
{
	public class VocabularyRegistry
	{
		private ConcurrentDictionary<Uri, Vocabulary> _vocabularies;

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
		}

		public void Register(Vocabulary vocabulary)
		{
			_vocabularies ??= new ConcurrentDictionary<Uri, Vocabulary>();
			_vocabularies[vocabulary.Id] = vocabulary;
		}

		public bool IsKnown(Uri vocabularyId)
		{
			if (_vocabularies != null && _vocabularies.ContainsKey(vocabularyId)) return true;

			if (!ReferenceEquals(this, Global))
				return Global._vocabularies.ContainsKey(vocabularyId);

			return false;
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