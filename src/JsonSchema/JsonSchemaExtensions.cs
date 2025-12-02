using System.Linq;

namespace Json.Schema;

public static class JsonSchemaExtensions
{
	public static KeywordData? GetKeyword<T>(this JsonSchemaNode schema)
		where T : IKeywordHandler =>
		schema.Keywords.FirstOrDefault(x => x.Handler is T);
}