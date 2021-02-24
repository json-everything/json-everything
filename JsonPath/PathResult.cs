using System.Collections.Generic;

namespace Json.Path
{
	/// <summary>
	/// The results of a JSON Path evaluation against a JSON instance.
	/// </summary>
	public class PathResult
	{
		/// <summary>
		/// The list of matches.
		/// </summary>
		public IReadOnlyList<PathMatch>? Matches { get; }
		/// <summary>
		/// An error, if any, that occurred during evaluation.
		/// </summary>
		public string? Error { get; }

		internal PathResult(IReadOnlyList<PathMatch> matches)
		{
			Matches = matches;
		}
		internal PathResult(string error)
		{
			Error = error;
		}
	}
}