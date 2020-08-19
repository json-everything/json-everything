using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema
{
	public static class KeywordExtensions
	{
		public static string Keyword(this IJsonSchemaKeyword keyword)
		{
			if (keyword == null) throw new ArgumentNullException(nameof(keyword));

			var keywordAttribute = keyword.GetType().GetCustomAttribute<SchemaKeywordAttribute>();
			if (keywordAttribute == null)
				throw new ArgumentException($"Keyword implementation `{keyword.GetType().Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

			return keywordAttribute.Name;
		}

		internal static long Priority(this IJsonSchemaKeyword keyword)
		{
			if (keyword == null) throw new ArgumentNullException(nameof(keyword));

			var priorityAttribute = keyword.GetType().GetCustomAttribute<SchemaPriorityAttribute>();
			if (priorityAttribute == null) return 0;

				return priorityAttribute.ActualPriority;
		}

		private static readonly Dictionary<Type, string> _attributes =
			typeof(IJsonSchemaKeyword).Assembly
				.GetTypes()
				.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
				            !t.IsAbstract &&
				            !t.IsInterface)
				.ToDictionary(t => t, t => t.GetCustomAttribute<SchemaKeywordAttribute>().Name);

		public static string Name(this IJsonSchemaKeyword keyword)
		{
			var keywordType = keyword.GetType();
			if (!_attributes.TryGetValue(keywordType, out var name))
			{
				name = keywordType.GetCustomAttribute<SchemaKeywordAttribute>()?.Name;
				if (name == null)
					throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaKeywordAttribute)}");

				_attributes[keywordType] = name;
			}

			return name;
		}
	}
}