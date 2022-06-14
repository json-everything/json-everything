using System.Text.RegularExpressions;

namespace JsonEverythingNet.Services.MarkdownGen
{
    internal static class StringExtensions
    {
        public static string RegexReplace(this string input, string pattern, string replacement)
            => Regex.Replace(input, pattern, replacement);
        public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);
    }
}
