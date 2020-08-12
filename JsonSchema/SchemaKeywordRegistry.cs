using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Json.Schema
{
	public static class SchemaKeywordRegistry
	{
		private static readonly ConcurrentDictionary<string, Type> _keywords;
		private static readonly ConcurrentDictionary<Type, IJsonSchemaKeyword> _nullKeywords;

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

			var nullElement = JsonDocument.Parse("null").RootElement;
			_nullKeywords = new ConcurrentDictionary<Type, IJsonSchemaKeyword>
			{
				[typeof(ConstKeyword)] = new ConstKeyword(nullElement),
				[typeof(DefaultKeyword)] = new DefaultKeyword(nullElement)
			};
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

		public static void RegisterNullValue<T>(T nullKeyword)
			where T : IJsonSchemaKeyword
		{
			_nullKeywords[typeof(T)] = nullKeyword;
		}

		internal static IJsonSchemaKeyword GetNullValuedKeyword(Type keywordType)
		{
			return _nullKeywords.TryGetValue(keywordType, out var instance) ? instance : null;
		}
	}
}