namespace Json.Path;

/// <summary>
/// The results of a JSON Path evaluation against a JSON instance.
/// </summary>
public class PathResult
{
	/// <summary>
	/// The list of matches.
	/// </summary>
	public NodeList Matches { get; }

	internal PathResult(NodeList matches)
	{
		Matches = matches;
	}
}
