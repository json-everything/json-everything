using Json.Path;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	public class PathQueryOutput
	{
		public PathResult Result { get; init; }
		public string Error { get; init; }
	}
}