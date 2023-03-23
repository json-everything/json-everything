using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class AddOperator : IBinaryValueOperator
{
	public int Precedence => 1;

	public PathValue? Evaluate(PathValue? left, PathValue? right)
	{
		if (left?.TryGetJson() is not JsonValue lValue ||
		    right?.TryGetJson() is not JsonValue rValue)
			return null;

		if (lValue.TryGetValue(out string? leftString) &&
		    rValue.TryGetValue(out string? rightString))
			return (JsonNode?)(leftString + rightString);

		return (JsonNode?)(lValue.GetNumber() + rValue.GetNumber());
	}

	public override string ToString()
	{
		return "+";
	}
}