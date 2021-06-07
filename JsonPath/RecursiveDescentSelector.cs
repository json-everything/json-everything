using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Path
{
	internal class RecursiveDescentSelector : SelectorBase
	{
		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			return GetChildren(match);
		}

		private static IEnumerable<PathMatch> GetChildren(PathMatch match)
		{
			switch (match.Value.ValueKind)
			{
				case JsonValueKind.Object:
					yield return match;
					foreach (var prop in match.Value.EnumerateObject())
					{
						var newMatch = new PathMatch(prop.Value, match.Location.Combine(PointerSegment.Create(prop.Name)));
						foreach (var child in GetChildren(newMatch))
						{
							yield return child;
						}
					}
					break;
				case JsonValueKind.Array:
					yield return match;
					foreach (var (item, index) in match.Value.EnumerateArray().Select((item, i) => (item, i)))
					{
						var newMatch = new PathMatch(item, match.Location.Combine(PointerSegment.Create($"{index}")));
						foreach (var child in GetChildren(newMatch))
						{
							yield return child;
						}
					}
					break;
				case JsonValueKind.String:
				case JsonValueKind.Number:
				case JsonValueKind.True:
				case JsonValueKind.False:
				case JsonValueKind.Null:
					yield return match;
					break;
				case JsonValueKind.Undefined:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override string ToString()
		{
			return ".";
		}
	}
}
