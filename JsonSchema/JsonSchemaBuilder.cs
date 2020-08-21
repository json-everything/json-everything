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

		public static JsonSchema RefRoot()
		{
			return new JsonSchemaBuilder().Ref(new Uri("#", UriKind.RelativeOrAbsolute)).Build();
		}

		public static JsonSchema RecursiveRefRoot()
		{
			return new JsonSchemaBuilder().RecursiveRef(new Uri("#", UriKind.RelativeOrAbsolute)).Build();
		}

		public JsonSchema Build()
		{
			var duplicates = _keywords.GroupBy(k => k.Value.GetType())
				.Where(g => g.Count() > 1)
				.Select(g => g.Key.GetCustomAttribute<SchemaKeywordAttribute>()?.Name)
				.ToList();
			if (duplicates.Any())
				throw new ArgumentException($"Found duplicate keywords: [{string.Join(", ", duplicates)}]");

			return new JsonSchema(_keywords.Values, null);
		}

		public static implicit operator JsonSchema(JsonSchemaBuilder builder)
		{
			return builder.Build();
		}
	}
}