using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class InOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		var jLeft = left?.TryGetJson();
		var jRight = right?.TryGetJson();

		IEnumerable<JsonNode?>? values = jRight switch
		{
			JsonArray arr => arr,
			JsonObject obj => obj.Select(x => x.Value),
			_ => null
		};

		if (values == null) return false;

		return values.Contains(jLeft, JsonNodeEqualityComparer.Instance);
	}

	public override string ToString()
	{
		return "<";
	}
}