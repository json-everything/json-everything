using System.Collections.Generic;

namespace Json.Path
{
	internal class RootNodeSelector : SelectorBase
	{
		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			return new[] {match};
		}

		public override string ToString()
		{
			return "$";
		}
	}
}