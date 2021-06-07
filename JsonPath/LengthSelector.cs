using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Path
{
	internal class LengthSelector : SelectorBase
	{
		public static LengthSelector Instance { get; } = new LengthSelector();

		private LengthSelector() { }

		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			switch (match.Value.ValueKind)
			{
				case JsonValueKind.Object:
					yield return new PathMatch(match.Value.EnumerateObject().Count().AsJsonElement(), match.Location.Combine(PointerSegment.Create("$length")));
					break;
				case JsonValueKind.Array:
					yield return new PathMatch(match.Value.GetArrayLength().AsJsonElement(), match.Location.Combine(PointerSegment.Create("$length")));
					break;
			}
		}

		public override string ToString()
		{
			return ".length";
		}
	}
}