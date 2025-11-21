using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Json.Schema.Keywords;

namespace Json.Schema;

/// <summary>
/// Manages which keywords are known by the system.
/// </summary>
public partial class SchemaKeywordRegistry
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
			
			//var nameAttribute = type.GetCustomAttribute<SchemaKeywordAttribute>() ??
			//                    throw new ArgumentException($"Keyword implementation `{type.Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");
			//Name = nameAttribute.Name;
			
			//var supportedVersionAttributes = type.GetCustomAttributes<SchemaSpecVersionAttribute>();
			//SupportedVersions = supportedVersionAttributes.Aggregate(SpecVersion.Unspecified, (total, current) => total | current.Version);
		}
	}

	private readonly MultiLookupConcurrentDictionary<KeywordMetaData> _keywordData;

	public static SchemaKeywordRegistry Default { get; set; } = Draft202012;

	public SchemaKeywordRegistry(params IKeywordHandler[] keywordData)
	{
		_keywordData = [];
		_keywordData.AddLookup(x => x.Name);
		_keywordData.AddLookup(x => x.Type);
		foreach (var type in keywordData)
		{
			var metaData = new KeywordMetaData(type);
			_keywordData.Add(metaData);
		}
	}

	/// <summary>
	/// Registers a new keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	public void Register<T>(T handler)
		where T : IKeywordHandler
	{
		_keywordData.Add(new KeywordMetaData(handler));

		EvaluateDependencies();
	}

	/// <summary>
	/// Unregisters a keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	public void Unregister<T>()
		where T : IKeywordHandler
	{
		if (_keywordData.TryGetValue(typeof(T), out var metaData))
			_keywordData.Remove(metaData);
	}

	public IKeywordHandler? GetHandler(string keyword)
	{
		return _keywordData.GetValueOrDefault(keyword)?.Handler;
	}

	internal void EvaluateDependencies()
	{
		var toCheck = _keywordData.Select(x => x.Value).Distinct().ToList();

		//var keyword = _keywordData[SchemaKeyword.Name];
		//keyword.Priority = -2;
		//toCheck.Remove(keyword);
		//keyword = _keywordData[IdKeyword.Name];
		//keyword.Priority = -1;
		//toCheck.Remove(keyword);
		//keyword = _keywordData[UnevaluatedItemsKeyword.Name];
		//keyword.Priority = long.MaxValue;
		//toCheck.Remove(keyword);
		//keyword = _keywordData[UnevaluatedPropertiesKeyword.Name];
		//keyword.Priority = long.MaxValue;
		//toCheck.Remove(keyword);

		var priority = 0;
		while (toCheck.Count != 0)
		{
			var unprioritized = toCheck.Select(x => x.Type).ToArray();
			for (var i = 0; i < toCheck.Count; i++)
			{
				var keyword = toCheck[i];
				var dependencies = keyword.Type.GetCustomAttributes<DependsOnAnnotationsFromAttribute>()
					.Select(x => x.DependentType);
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
}