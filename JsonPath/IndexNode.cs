using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace JsonPath
{
	internal class IndexNode : PathNodeBase
	{
		public IReadOnlyList<IIndexExpression> Ranges { get; }

		public IndexNode(IEnumerable<IIndexExpression> ranges)
		{
			Ranges = ranges.ToList();
		}

		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			if (match.Value.ValueKind != JsonValueKind.Array) yield break;

			var array = match.Value.EnumerateArray().ToArray();
			var indices = Ranges.SelectMany(r => r.GetIndices(match.Value))
				.OrderBy(i => i)
				.Distinct();
			foreach (var index in indices)
			{
				yield return new PathMatch(array[index], match.Location.Combine(PointerSegment.Create($"{index}")));
			}
		}
	}
}