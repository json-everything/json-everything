using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Json.Schema
{
	public static class SchemaKeywordRegistry
	{
		private static readonly ConcurrentDictionary<string, Type> _keywords;

		static SchemaKeywordRegistry()
		{
			_keywords = new ConcurrentDictionary<string, Type>(
				typeof(SchemaKeywordRegistry)
					.Assembly
					.GetTypes()
					.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
					            t.GetCustomAttribute<SchemaKeywordAttribute>() != null)
					.Select(t => new {Type = t, Keyword = t.GetCustomAttribute<SchemaKeywordAttribute>().Name})
					.ToDictionary(k => k.Keyword, k => k.Type));
		}

		public static void Register<T>()
			where T : IJsonSchemaKeyword
		{
			var keyword = typeof(T).GetCustomAttribute<SchemaKeywordAttribute>();
			if (keyword == null)
				throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

			_keywords[keyword.Name] = typeof(T);
		}

		public static Type GetImplementationType(string keyword)
		{
			return _keywords.TryGetValue(keyword, out var implementationType)
				? implementationType
				: null;
		}
	}
}