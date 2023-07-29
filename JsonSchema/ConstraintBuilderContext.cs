using System;
using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema;

public class ConstraintBuilderContext
{
	private readonly Dictionary<Uri, Vocabulary[]?> _schemaVocabs = new();

	public EvaluationOptions Options { get; }
	public DynamicScope Scope { get; }

	internal SpecVersion EvaluatingAs { get; }
	internal Stack<(string, JsonPointer)> NavigatedReferences { get; } = new();

	internal ConstraintBuilderContext(EvaluationOptions options, SpecVersion evaluatingAs, Uri initialScope)
	{
		Options = options;
		EvaluatingAs = evaluatingAs;
		Scope = new DynamicScope(initialScope);
	}

	internal IEnumerable<IJsonSchemaKeyword> GetKeywordsToProcess(JsonSchema schema)
	{
		if (!TryGetVocab(schema, out var vocab) || vocab == null) return schema.Keywords!;

		var vocabKeywordTypes = vocab.SelectMany(x => x.Keywords);
		return schema.Keywords!.Where(x => vocabKeywordTypes.Contains(x.GetType()));
	}

	private bool TryGetVocab(JsonSchema schema, out Vocabulary[]? vocab)
	{
		if (_schemaVocabs.TryGetValue(schema.BaseUri, out vocab)) return true;

		_schemaVocabs[schema.BaseUri] = null;

		var schemaKeyword = (SchemaKeyword?)schema.Keywords?.FirstOrDefault(x => x is SchemaKeyword);
		if (schemaKeyword == null) return false;

		var metaSchema = (JsonSchema) Options.SchemaRegistry.Get(schemaKeyword.Schema)!;
		var vocabKeyword = (VocabularyKeyword?)metaSchema.Keywords!.FirstOrDefault(x => x is VocabularyKeyword);
		if (vocabKeyword == null) return false;

		vocab = vocabKeyword.Vocabulary.Keys.Select(x => Options.VocabularyRegistry.Get(x)!).ToArray();
		_schemaVocabs[schema.BaseUri] = vocab;
		return true;
	}
}