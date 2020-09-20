using System.Collections.Generic;

namespace Json.Path
{
	internal class RootNode : PathNodeBase
	{
		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			return new[] {match};
		}
	}
}