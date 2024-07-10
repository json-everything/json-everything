using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public static partial class Vocabularies
{
	private static readonly Dictionary<string, Vocabulary> _registry = [];
	private static readonly ConcurrentDictionary<string, Dictionary<string, IKeywordHandler>> _keywordsByMetaschema = [];

	static Vocabularies()
	{
		Register(Core201909,
			Applicator201909,
			Validation201909,
			MetaData201909,
			Format201909,
			Content201909,
			Core202012,
			Applicator202012,
			Unevaluated202012,
			Validation202012,
			MetaData202012,
			FormatAnnotation202012,
			FormatAssertion202012,
			Content202012,
			CoreNext,
			ApplicatorNext,
			UnevaluatedNext,
			ValidationNext,
			MetaDataNext,
			FormatAnnotationNext,
			FormatAssertionNext,
			ContentNext);
	}

	public static void Register(params Vocabulary[] vocabularies)
	{
		foreach (var vocabulary in vocabularies)
		{
			_registry[vocabulary.Id.OriginalString] = vocabulary;
		}
		KeywordRegistry.Register(vocabularies);
	}

	internal static IReadOnlyDictionary<string, IKeywordHandler>? GetHandlersByMetaschema(JsonObject metaschema, EvaluationContext context)
	{
		var id = (metaschema["$id"] as JsonValue)?.GetString();
		if (id is null) return null;

		if (!_keywordsByMetaschema.TryGetValue(id, out var handlers))
		{
			if (!metaschema.TryGetValue("$vocabulary", out var vocabularyNode, out _)) return null;
			if (vocabularyNode is not JsonObject vocabObject)
				throw new SchemaValidationException("$vocabulary must be an object", context);

			var vocabIds = vocabObject.ToDictionary(x => x.Key, x => (x.Value as JsonValue)?.GetBool());
			if (vocabIds.Any(x => !Uri.IsWellFormedUriString(x.Key, UriKind.Absolute) || x.Value is null))
				throw new SchemaValidationException("$vocabulary must contain absolute URI keys with boolean values", context);

			var vocabs = vocabIds
				.Select(x => x.Value == true ? Get(x.Key) : TryGet(x.Key))
				.Where(x => x is not null)
				.ToArray();

			_keywordsByMetaschema[id] = handlers = vocabs.SelectMany(x => x!.Handlers).ToDictionary(x => x.Name);
		}

		return handlers;
	}

	private static Vocabulary Get(string vocabId)
	{
		return TryGet(vocabId) ?? throw new ArgumentException($"Required vocabulary '{vocabId}' not recognized");
	}

	private static Vocabulary? TryGet(string vocabId)
	{
		return _registry.GetValueOrDefault(vocabId);
	}
}