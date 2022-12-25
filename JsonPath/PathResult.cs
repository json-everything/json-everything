namespace Json.Path;

/// <summary>
/// The results of a JSON Path evaluation against a JSON instance.
/// </summary>
public class PathResult
{
	/// <summary>
	/// The list of matches.
	/// </summary>
	public NodeList? Matches { get; }
	/// <summary>
	/// An error, if any, that occurred during evaluation.
	/// </summary>
	public string? Error { get; }

	internal PathResult(NodeList matches)
	{
		Matches = matches;
	}
	internal PathResult(string error)
	{
		Error = error;
	}
}