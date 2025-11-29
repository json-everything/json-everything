using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Json.Schema.Keywords;

namespace Json.Schema;

/// <summary>
/// Represents a JSON Schema dialect, defining the set of supported keywords and validation rules for a specific schema
/// version.
/// </summary>
/// <remarks>A dialect encapsulates the behavior and interpretation of JSON Schema keywords according to a
/// particular specification draft. Use a specific dialect instance, such as <see cref="Draft06"/>, to validate or
/// process schemas that conform to that draft. Dialects may differ in supported keywords, validation semantics, and
/// handling of unknown keywords. This class is typically used to select the appropriate schema version when working
/// with JSON Schema documents.</remarks>
public partial class Dialect
{
	[DebuggerDisplay("{Name} / {Priority}")]
	private class KeywordMetaData
	{
		public string Name { get; }
		public Type Type { get; }
		public IKeywordHandler Handler { get; }
		// ReSharper disable MemberHidesStaticFromOuterClass
		public long Priority { get; set; }
		public bool ProducesDependentAnnotations { get; set; }
		// ReSharper restore MemberHidesStaticFromOuterClass

		public KeywordMetaData(IKeywordHandler handler)
		{
			Name = handler.Name;
			Handler = handler;
			Type = handler.GetType();
		}
	}

	private readonly MultiLookupConcurrentDictionary<KeywordMetaData> _keywordData;
	private bool _readOnly;

	public static Dialect Default { get; set; } = null!;

	public bool RefIgnoresSiblingKeywords
	{
		get;
		init
		{
			CheckWellKnown();
			field = value;
		}
	}

	public bool AllowUnknownKeywords
	{
		get;
		init
		{
			CheckWellKnown();
			field = value;
		}
	}

	public Uri? Id { get; init; }

	public Dialect(params IEnumerable<IKeywordHandler> keywords)
	{
		_keywordData = [];
		_keywordData.AddLookup(x => x.Name);
		_keywordData.AddLookup(x => x.Type);
		foreach (var keyword in keywords)
		{
			var keywordData = new KeywordMetaData(keyword);
			_keywordData.Add(keywordData);
		}
		EvaluateDependencies();
	}

	internal Dialect(IEnumerable<Vocabulary> vocabs)
	{
		_keywordData = [];
		_keywordData.AddLookup(x => x.Name);
		_keywordData.AddLookup(x => x.Type);
		foreach (var vocab in vocabs)
		{
			foreach (var keyword in vocab.Keywords)
			{
				var metaData = new KeywordMetaData(keyword);
				_keywordData.Add(metaData);
			}
		}
		EvaluateDependencies();
	}

	internal IKeywordHandler GetHandler(string keyword)
	{
		var handler = _keywordData.GetValueOrDefault(keyword)?.Handler;

		return handler ?? (AllowUnknownKeywords
			? AnnotationKeyword.Instance
			: throw new JsonSchemaException($"Unknown keywords ({keyword}) are disallowed for this dialect."));
	}

	private void EvaluateDependencies()
	{
		var toCheck = _keywordData.Select(x => x.Value).Distinct().ToList();

		if (_keywordData.TryGetValue("$ref", out var keyword) && RefIgnoresSiblingKeywords)
		{
			keyword.Priority = -4;
			toCheck.Remove(keyword);
		}
		if (_keywordData.TryGetValue("$schema", out keyword))
		{
			keyword.Priority = -3;
			toCheck.Remove(keyword);
		}
		if (_keywordData.TryGetValue("$vocabulary", out keyword))
		{
			keyword.Priority = -2;
			toCheck.Remove(keyword);
		}
		if (_keywordData.TryGetValue("$id", out keyword))
		{
			keyword.Priority = -1;
			toCheck.Remove(keyword);
		}
		if (_keywordData.TryGetValue("unevaluatedItems", out keyword))
		{
			keyword.Priority = long.MaxValue;
			toCheck.Remove(keyword);
		}
		if (_keywordData.TryGetValue("unevaluatedProperties", out keyword))
		{
			keyword.Priority = long.MaxValue;
			toCheck.Remove(keyword);
		}

		var priority = 0;
		while (toCheck.Count != 0)
		{
			var unprioritized = toCheck.Select(x => x.Type).ToArray();
			for (var i = 0; i < toCheck.Count; i++)
			{
				keyword = toCheck[i];
				var dependencies = keyword.Type
					.GetCustomAttributes<DependsOnAnnotationsFromAttribute>()
					.Select(x => x.DependentType)
					.ToArray();
				foreach (var dependency in dependencies)
				{
					var metaData = _keywordData[dependency];
					metaData.ProducesDependentAnnotations = true;
				}

				var matches = dependencies.Intersect(unprioritized);
				if (matches.Any()) continue;

				keyword.Priority = priority;
				toCheck.Remove(keyword);
				i--;
			}

			priority++;
		}
	}

	internal long? GetEvaluationOrder(string keyword)
	{
		return _keywordData.GetValueOrDefault(keyword)?.Priority;
	}

	private void CheckWellKnown()
	{
		if (_readOnly)
			throw new InvalidOperationException("Editing the well-known keyword registries is not permitted.");
	}
}
