using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class AddOperator : IBinaryOperator
{
	public int Precedence => 7;

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

internal class ExponentOperator : IBinaryOperator
{
	public int Precedence => 7;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return null;

		var dLeft = (double?) lValue.GetNumber();
		var dRight = (double?) rValue.GetNumber();

		return dLeft.HasValue && dRight.HasValue
			? (decimal)Math.Pow(dLeft.Value, dRight.Value)
			: null;
	}

	public override string ToString()
	{
		return "**";
	}
}