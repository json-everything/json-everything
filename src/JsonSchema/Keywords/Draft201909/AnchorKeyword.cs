using System.Text.RegularExpressions;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `$anchor`.
/// </summary>
/// <remarks>
/// This keyword is used to define a location-independent identifier that can be referenced from elsewhere in the schema.
/// </remarks>
public partial class AnchorKeyword : Json.Schema.Keywords.AnchorKeyword
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="AnchorKeyword"/>.
	/// </summary>
	public new static AnchorKeyword Instance { get; } = new();

#if NET7_0_OR_GREATER
	/// <summary>
	/// The pattern for valid anchor identifiers.
	/// </summary>
	public override Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	/// <summary>
	/// The pattern for valid anchor identifiers.
	/// </summary>
	public override Regex AnchorPattern { get; } = new("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled);
#endif

	/// <summary>
	/// Initializes a new instance of the <see cref="AnchorKeyword"/> class.
	/// </summary>
	protected AnchorKeyword()
	{
	}
}
