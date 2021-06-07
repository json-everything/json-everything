using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Path
{
	internal class PropertySelector : SelectorBase
	{
		private readonly string? _name;

		public PropertySelector(string? name)
		{
			_name = name;
		}

		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			if (_name == null)
			{
				switch (match.Value.ValueKind)
				{
					case JsonValueKind.Object:
						foreach (var propPair in match.Value.EnumerateObject())
						{
							yield return new PathMatch(propPair.Value, match.Location.Combine(PointerSegment.Create(propPair.Name)));
						}
						break;
					case JsonValueKind.Array:
						foreach (var (value, index) in match.Value.EnumerateArray().Select((v, i) => (v, i)))
						{
							yield return new PathMatch(value, match.Location.Combine(PointerSegment.Create(index.ToString())));
						}
						break;
				}

				yield break;
			}

			if (match.Value.ValueKind != JsonValueKind.Object) yield break;

			if (!match.Value.TryGetProperty(_name, out var prop)) yield break;

			yield return new PathMatch(prop, match.Location.Combine(PointerSegment.Create(_name)));
		}

		public override string ToString()
		{
			return _name == null ? ".*" : $".{_name}";
		}
	}
}