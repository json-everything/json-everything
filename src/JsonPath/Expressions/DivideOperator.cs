using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class DivideOperator : IBinaryValueOperator
{
	public int Precedence => 2;

	public PathValue? Evaluate(PathValue? left, PathValue? right)
	{
		if (left is null || right is null) return null;

		if (!left.TryGetJson(out var lNode) || lNode is not JsonValue lValue ||
		    !right.TryGetJson(out var rNode) || rNode is not JsonValue rValue)
			return null;

		var rNumber = rValue.GetNumber();

		return rNumber is null or 0
			? null
			: (JsonNode?)(lValue.GetNumber() / rNumber);
	}

	public override string ToString()
	{
		return "/";
	}
}