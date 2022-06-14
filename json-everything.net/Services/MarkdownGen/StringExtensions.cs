using System.Text.RegularExpressions;

namespace JsonEverythingNet.Services.MarkdownGen;

internal static class StringExtensions
{
	public static string RegexReplace(this string input, string pattern, string replacement)
	{
		return Regex.Replace(input, pattern, replacement);
	}

	public static bool IsNullOrEmpty(this string? input)
	{
		return string.IsNullOrEmpty(input);
	}
}