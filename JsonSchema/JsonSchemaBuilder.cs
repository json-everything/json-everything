using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema
{
	public class JsonSchemaBuilder
	{
		private readonly Dictionary<string, IJsonSchemaKeyword> _keywords = new Dictionary<string, IJsonSchemaKeyword>();

		public void Add(IJsonSchemaKeyword keyword)
		{
			_keywords[keyword.Keyword()] = keyword;
		}

		public JsonSchema Build()
		{
			var duplicates = _keywords.GroupBy(k => k.GetType())
				.Where(g => g.Count() > 1)
				.Select(g => g.Key.GetCustomAttribute<SchemaKeywordAttribute>()?.Name)
				.ToList();
			if (duplicates.Any())
				throw new ArgumentException($"Found duplicate keywords: [{string.Join(", ", duplicates)}]");

			return new JsonSchema(_keywords.Values, null);
		}
	}
}