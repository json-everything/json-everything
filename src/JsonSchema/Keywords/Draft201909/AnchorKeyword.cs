using System.Text.RegularExpressions;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `$anchor`.
/// </summary>
public partial class AnchorKeyword : Json.Schema.Keywords.AnchorKeyword
{
#if NET7_0_OR_GREATER
	public override Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	public override Regex AnchorPattern { get; } = new("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled);
#endif
}
