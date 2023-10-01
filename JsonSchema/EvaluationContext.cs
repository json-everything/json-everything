using System;
using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides contextual data for generating constraints and performing evaluations.
/// </summary>
public class EvaluationContext
{
	private readonly Stack<SpecVersion> _evaluatingAs = new();

	/// <summary>
	/// Gets the evaluation options.
	/// </summary>
	/// <remarks>
	/// This may be different per run, so it's important this not be captured by constraints.
	/// </remarks>
	public EvaluationOptions Options { get; }
	
	/// <summary>
	/// Gets the dynamic scope.
	/// </summary>
	/// <remarks>
	/// The dynamic scope is the collection of URIs that evaluation has passed through to get
	/// to the current location.  This is important when processing references.
	/// </remarks>
	public DynamicScope Scope { get; }
	
	/// <summary>
	/// The vocabularies defined by the schema in the local scope.
	/// </summary>
	/// <remarks>
	/// Due to historic evolution of JSON Schema, some keywords (particularly `format`) require
	/// knowledge of the active vocabularies in order to process correctly.
	/// </remarks>
	public Dictionary<Uri, Vocabulary[]?> Dialect { get; } = new();
	
	/// <summary>
	/// Gets the spec version that the schema is currently being evaluated under.
	/// </summary>
	/// <remarks>
	/// This property is informed by the `$schema` keyword and <see cref="EvaluationOptions.EvaluateAs"/>,
	/// taking `$schema` as priority.
	/// </remarks>
	public SpecVersion EvaluatingAs { get; private set; }

	internal Stack<(string, JsonPointer)> NavigatedReferences { get; } = new();

	internal EvaluationContext(EvaluationOptions options, SpecVersion evaluatingAs, Uri initialScope)
	{
		Options = options;
		PushEvaluatingAs(evaluatingAs);
		Scope = new DynamicScope(initialScope);
	}

	internal IEnumerable<IJsonSchemaKeyword> GetKeywordsToProcess(JsonSchema schema, EvaluationOptions options)
	{
		if (options.ProcessCustomKeywords ||
		    !TryGetVocab(schema, out var vocab) ||
		    vocab == null) return schema.Keywords!;

		var vocabKeywordTypes = vocab.SelectMany(x => x?.Keywords ?? Array.Empty<Type>());
		return schema.Keywords!.Where(x => vocabKeywordTypes.Contains(x.GetType()));
	}

	private bool TryGetVocab(JsonSchema schema, out Vocabulary[]? vocab)
	{
		if (Dialect.TryGetValue(schema.BaseUri, out vocab)) return true;

		Dialect[schema.BaseUri] = null;

		var schemaKeyword = (SchemaKeyword?)schema.Keywords?.FirstOrDefault(x => x is SchemaKeyword);
		if (schemaKeyword == null) return false;

		var metaSchema = (JsonSchema) Options.SchemaRegistry.Get(schemaKeyword.Schema)!;
		var vocabKeyword = (VocabularyKeyword?)metaSchema.Keywords!.FirstOrDefault(x => x is VocabularyKeyword);
		if (vocabKeyword == null) return false;

		vocab = vocabKeyword.Vocabulary.Keys.Select(x => Options.VocabularyRegistry.Get(x)!).ToArray();
		Dialect[schema.BaseUri] = vocab;
		return true;
	}

	internal void PushEvaluatingAs(SpecVersion version)
	{
		_evaluatingAs.Push(version);
		EvaluatingAs = version;
	}

	internal void PopEvaluatingAs()
	{ 
		_evaluatingAs.Pop();
		EvaluatingAs = _evaluatingAs.Peek();
	}
}