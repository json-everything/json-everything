using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Path;

internal class LengthSelector : SelectorBase
{
	public static LengthSelector Instance { get; } = new();

	private LengthSelector() { }

	protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
	{
		switch (match.Value.ValueKind)
		{
			case JsonValueKind.Object:
				yield return new PathMatch(match.Value.EnumerateObject().Count().AsJsonElement(),
					match.Location.AddSelector(new PropertySelector("length")));
				break;
			case JsonValueKind.Array:
				yield return new PathMatch(match.Value.GetArrayLength().AsJsonElement(),
					match.Location.AddSelector(new PropertySelector("length")));
				break;
		}
	}

	public override string ToString()
	{
		return ".length";
	}
}