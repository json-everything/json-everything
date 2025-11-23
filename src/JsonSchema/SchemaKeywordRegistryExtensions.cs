using Json.Schema.Keywords;

namespace Json.Schema;

public static class SchemaKeywordRegistryExtensions
{
	public static SchemaKeywordRegistry UseFormatValidation(this SchemaKeywordRegistry source)
	{
		var oldFormatKeyword = source.GetHandler("format");
		if (oldFormatKeyword is FormatKeyword) return source;

		var copy = new SchemaKeywordRegistry(source);
		copy.Unregister<Keywords.Draft06.FormatKeyword>();
		copy.Register(new FormatKeyword());

		return copy;
	}
}