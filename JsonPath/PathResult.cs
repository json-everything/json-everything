using System.Collections.Generic;

namespace JsonPath
{
	public class PathResult
	{
		public IReadOnlyList<PathMatch> Matches { get; }
		public string Error { get; }

		public PathResult(IReadOnlyList<PathMatch> matches)
		{
			Matches = matches;
		}
		public PathResult(string error)
		{
			Error = error;
		}
	}
}