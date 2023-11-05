using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class SubtractOperator : IBinaryOperator
{
	public int Precedence => 7;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return null;

		return lValue.GetNumber() - rValue.GetNumber();
	}

	public override string ToString()
	{
		return "-";
	}
}