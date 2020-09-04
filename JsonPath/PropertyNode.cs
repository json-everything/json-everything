using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace JsonPath
{
	internal class PropertyNode : PathNodeBase
	{
		public string Name { get; }

		public PropertyNode(string name)
		{
			Name = name;
		}

		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			if (match.Value.ValueKind != JsonValueKind.Object) yield break;

			if (Name == null)
			{
				foreach (var propPair in match.Value.EnumerateObject())
				{
					yield return new PathMatch(propPair.Value, match.Location.Combine(PointerSegment.Create(propPair.Name)));
				}
				yield break;
			}

			if (!match.Value.TryGetProperty(Name, out var prop)) yield break;

			yield return new PathMatch(prop, match.Location.Combine(PointerSegment.Create(Name)));
		}
	}
}