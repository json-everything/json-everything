using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Json.Schema
{
	/// <summary>
	/// Manages which keywords are known by the system.
	/// </summary>
	/// <remarks>
	/// Because the deserialization process relies on keywords being registered,
	/// this class cannot be an instance class like the other registries in this
	/// library.  Therefore keywords are registered for all schemas.
	/// </remarks>
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

			using var document = JsonDocument.Parse("null");
			var nullElement = document.RootElement;
			_nullKeywords = new ConcurrentDictionary<Type, IJsonSchemaKeyword>
			{
				[typeof(ConstKeyword)] = new ConstKeyword(nullElement),
				[typeof(DefaultKeyword)] = new DefaultKeyword(nullElement)
			};
		}

		/// <summary>
		/// Registers a new keyword type.
		/// </summary>
		/// <typeparam name="T">The keyword type.</typeparam>
		public static void Register<T>()
			where T : IJsonSchemaKeyword, IEquatable<T>
		{
			var keyword = typeof(T).GetCustomAttribute<SchemaKeywordAttribute>();
			if (keyword == null)
				throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

			_keywords[keyword.Name] = typeof(T);
		}

		/// <summary>
		/// Unregisters a keyword type.
		/// </summary>
		/// <typeparam name="T">The keyword type.</typeparam>
		public static void Unregister<T>()
			where T : IJsonSchemaKeyword
		{
			var keyword = typeof(T).GetCustomAttribute<SchemaKeywordAttribute>();
			if (keyword == null)
				throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

			_keywords.TryRemove(keyword.Name, out _);
		}

		/// <summary>
		/// Gets the implementation for a given keyword name.
		/// </summary>
		/// <param name="keyword">The keyword name.</param>
		/// <returns>The keyword type, if registered; otherwise null.</returns>
		public static Type? GetImplementationType(string keyword)
		{
			return _keywords.TryGetValue(keyword, out var implementationType)
				? implementationType
				: null;
		}

		/// <summary>
		/// Registers a null-value for a keyword.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nullKeyword"></param>
		/// <remarks>
		/// This is important for keywords that accept null, like `default` and `const`.  Without
		/// this step, the serializer will skip keywords that have nulls.
		/// </remarks>
		public static void RegisterNullValue<T>(T nullKeyword)
			where T : IJsonSchemaKeyword
		{
			_nullKeywords[typeof(T)] = nullKeyword;
		}

		internal static IJsonSchemaKeyword? GetNullValuedKeyword(Type keywordType)
		{
			return _nullKeywords.TryGetValue(keywordType, out var instance) ? instance : null;
		}
	}
}