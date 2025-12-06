using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Keywords.Draft201909;

namespace Json.Schema;

/// <summary>
/// Provides a registry for managing and retrieving JSON Schema dialects by their unique identifiers.
/// </summary>
/// <remarks>The registry allows registration and unregistration of custom dialects. A global singleton instance is available via the <see
/// cref="Global"/> property for application-wide access. Getting dialects via
/// local instances fall back to the global registry.</remarks>
public class DialectRegistry
{
	private readonly Dictionary<Uri, Dialect> _dialects = new();
	private Uri[] _wellKnownDialects = null!;

	/// <summary>
	/// Gets the global registry of JSON Schema dialects used throughout the application.
	/// </summary>
	/// <remarks>Use this property to access or configure JSON Schema dialects that should be available application-wide.
	/// Changes to the global registry affect all components that rely on shared dialect definitions.</remarks>
	public static DialectRegistry Global { get; } = new();

	/// <summary>
	/// Registers a custom dialect for use in the system.
	/// </summary>
	/// <remarks>Registering a dialect allows it to be used in subsequent operations. Official dialects cannot be
	/// overwritten or replaced.</remarks>
	/// <param name="dialect">The dialect to register. The dialect must have a non-null identifier and must not conflict with any official
	/// dialects.</param>
	/// <exception cref="ArgumentException">Thrown if <paramref name="dialect"/> has a null <c>Id</c>, or if the dialect's identifier matches an official
	/// dialect.</exception>
	public void Register(Dialect dialect)
	{
		if (dialect.Id is null)
			throw new ArgumentException("Cannot register a dialect without an ID");

		if (_wellKnownDialects.Contains(dialect.Id))
			throw new ArgumentException("Cannot overwrite official dialects");

		_dialects[dialect.Id] = dialect;
	}

	/// <summary>
	/// Unregisters a dialect identified by the specified URI, removing it from the collection of available dialects.
	/// </summary>
	/// <param name="dialectId">The URI that uniquely identifies the dialect to unregister. Cannot be an official dialect.</param>
	/// <exception cref="ArgumentException">Thrown if <paramref name="dialectId"/> refers to an official dialect, which cannot be removed.</exception>
	public void Unregister(Uri dialectId)
	{
		if (_wellKnownDialects.Contains(dialectId))
			throw new ArgumentException("Cannot remove official dialects");

		_dialects.Remove(dialectId);
	}

	internal Dialect Get(Uri uri, SchemaRegistry schemaRegistry, VocabularyRegistry vocabularyRegistry, Dialect basis)
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

		dialect = new Dialect(keywords)
		{
			Id = uri,
			AllowUnknownKeywords = basis.AllowUnknownKeywords,
			RefIgnoresSiblingKeywords = basis.RefIgnoresSiblingKeywords
		};
		_dialects[uri] = dialect;

		return dialect;
	}

	internal void RegisterDefaultDialects()
	{
		_dialects[MetaSchemas.Draft6Id] = Dialect.Draft06;
		_dialects[MetaSchemas.Draft7Id] = Dialect.Draft07;
		_dialects[MetaSchemas.Draft201909Id] = Dialect.Draft201909;
		_dialects[MetaSchemas.Draft202012Id] = Dialect.Draft202012;
		_dialects[MetaSchemas.V1_2026Id] = Dialect.V1_2026;

		_dialects[MetaSchemas.V1Id] = Dialect.V1;

		_wellKnownDialects = _dialects.Keys.ToArray();
	}
}