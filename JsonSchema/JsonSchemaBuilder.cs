using System.Collections.Generic;

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
			return new JsonSchema(_keywords.Values);
		}
	}
}