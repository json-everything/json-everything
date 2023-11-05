using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class LessThanOperator : IBinaryOperator
{
	public int Precedence => 3;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return false;

		if (lValue.TryGetValue(out string? leftString) &&
		    rValue.TryGetValue(out string? rightString))
			return string.Compare(leftString, rightString, StringComparison.Ordinal) < 0;

		return lValue.GetNumber() < rValue.GetNumber();
	}

	public override string ToString()
	{
		return "<";
	}
}