using Json.Schema.Keywords;

namespace Json.Schema;

public static class DialectExtensions
{
	public static Dialect UseFormatValidation(this Dialect source)
	{
		var oldFormatKeyword = source.GetHandler("format");
		if (oldFormatKeyword is FormatKeyword) return source;

		var copy = new Dialect(source);
		copy.Unregister<Keywords.Draft06.FormatKeyword>();
		copy.Register(new FormatKeyword());

		return copy;
	}
}