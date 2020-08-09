using System;
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
	}
}