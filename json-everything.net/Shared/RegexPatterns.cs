using System.Text.RegularExpressions;

namespace JsonEverythingNet.Shared;

internal static class RegexPatterns
{
	public static readonly Regex HeaderPattern = new(@"<h(\d) id=""([-.a-z0-9]+)"">\s*(.*)\s*<\/h\d>", RegexOptions.Compiled);
}