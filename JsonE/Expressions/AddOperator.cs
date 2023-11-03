using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class AddOperator : IBinaryValueOperator
{
	public int Precedence => 1;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return null;

		if (lValue.TryGetValue(out string? leftString) &&
		    rValue.TryGetValue(out string? rightString))
			return leftString + rightString;

		return lValue.GetNumber() + rValue.GetNumber();
	}

	public override string ToString()
	{
		return "+";
	}
}