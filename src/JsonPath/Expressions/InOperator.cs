using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class InOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		if (left is null) return right is null;
		if (right is null) return false;

		if (!left.TryGetJson(out var lNode) ||
		    !right.TryGetJson(out var rNode))
			return false;

		var values = rNode switch
		{
			JsonArray arr => arr,
			JsonObject obj => obj.Select(x => x.Value),
			_ => null
		};

		if (values == null) return false;

		return values.Contains(lNode, JsonNodeEqualityComparer.Instance);
	}

	public override string ToString()
	{
		return "in";
	}
}
