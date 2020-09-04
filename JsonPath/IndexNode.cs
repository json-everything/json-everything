using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace JsonPath
{
	internal class IndexNode : PathNodeBase
	{
		public IReadOnlyList<IndexOrRange> Ranges { get; }

		public IndexNode(IEnumerable<IndexOrRange> ranges)
		{
			Ranges = ranges.ToList();
		}

		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			if (match.Value.ValueKind != JsonValueKind.Array) yield break;

			var array = match.Value.EnumerateArray().ToArray();
			foreach (var indexOrRange in Ranges)
			{
				if (indexOrRange.IsRange)
				{
					var start = indexOrRange.Range.Start.IsFromEnd
						? array.Length - indexOrRange.Range.Start.Value
						: indexOrRange.Range.Start.Value;
					var i = 0;
					foreach (var item in array[indexOrRange.Range])
					{
						yield return new PathMatch(item, match.Location.Combine(PointerSegment.Create($"{i + start}")));
					}
				}
				else
				{
					var index = indexOrRange.Index.IsFromEnd
						? array.Length - indexOrRange.Index.Value
						: indexOrRange.Index.Value;
					yield return new PathMatch(array[indexOrRange.Index], match.Location.Combine(PointerSegment.Create($"{index}")));
				}
			}
		}
	}
}