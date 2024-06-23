using System;

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
	/// <summary>
	/// An error, if any, that occurred during evaluation.
	/// </summary>
	[Obsolete("JSON Path must not error during evaluation.  This is never used.  The next major version will replace the PathResult type with just a NodeList.")]
	public string? Error { get; }

	internal PathResult(NodeList matches)
	{
		Matches = matches;
	}
	[Obsolete]
	internal PathResult(string error)
	{
		Matches = [];
		Error = error;
	}
}
