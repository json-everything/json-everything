using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Keywords;

namespace Json.Schema;

public class DialectRegistry
{
	private readonly Dictionary<Uri, Dialect> _dialects = new();
	private Uri[] _wellKnownDialects = null!;

	public static DialectRegistry Global { get; } = new();

	public void Register(Dialect dialect)
	{
		if (dialect.Id is null)
			throw new ArgumentException("Cannot register a dialect without an ID");

		if (_wellKnownDialects.Contains(dialect.Id))
			throw new ArgumentException("Cannot overwrite official dialects");

		_dialects[dialect.Id] = dialect;
	}

	public void Unregister(Uri dialectId)
	{
		if (_wellKnownDialects.Contains(dialectId))
			throw new ArgumentException("Cannot remove official dialects");

		_dialects.Remove(dialectId);
	}

	internal Dialect Get(Uri uri, SchemaRegistry schemaRegistry, VocabularyRegistry vocabularyRegistry)
	{
		var dialect = _dialects.GetValueOrDefault(uri) ??
			Global._dialects.GetValueOrDefault(uri);
		if (dialect is not null) return dialect;

		var document = schemaRegistry.Get(uri);
		if (document is not JsonSchema schema)
			throw new JsonSchemaException($"Cannot find dialect with ID '{uri}'. " +
			                              $"If the meta-schema is based on Draft 2019-09 or Draft 2020-12 and carries a $vocabulary keyword, " +
			                              $"simply add the meta-schema to the schema registry. " +
			                              $"Otherwise, explicitly register the dialect.");

		var vocabKeyword = schema.Root.Keywords?.FirstOrDefault(x => x.Handler is VocabularyKeyword);
		if (vocabKeyword is null)
			throw new JsonSchemaException($"Cannot automatically determine keywords for dialect with ID '{uri}'. " +
			                              $"Register it explicitly.");

		var keywords = new List<IKeywordHandler>();
		foreach (var vocabEntry in vocabKeyword.RawValue.EnumerateObject())
		{
			var vocab = vocabularyRegistry.GetVocab(new Uri(vocabEntry.Name));
			if (vocab is null)
			{
				if (vocabEntry.Value.GetBoolean())
					throw new JsonSchemaException($"Vocabulary with ID '{vocabEntry.Name}' is required but not known.");
				continue;
			}

			keywords.AddRange(vocab.Keywords);
		}

		dialect = new Dialect(keywords){Id = uri};
		_dialects[uri] = dialect;

		return dialect;
	}

	internal void RegisterDefaultDialects()
	{
		_dialects[MetaSchemas.Draft6Id] = Dialect.Draft06;
		_dialects[MetaSchemas.Draft7Id] = Dialect.Draft07;
		_dialects[MetaSchemas.Draft201909Id] = Dialect.Draft201909;
		_dialects[MetaSchemas.Draft202012Id] = Dialect.Draft202012;
		_dialects[MetaSchemas.V1_2026Id] = Dialect.V1;
		_dialects[MetaSchemas.V1Id] = Dialect.V1;

		_wellKnownDialects = _dialects.Keys.ToArray();
	}
}