using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class LessThanOrEqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left.IsEquivalentTo(right)) return true;

		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return false;

		if (lValue.TryGetValue(out string? leftString) &&
		    rValue.TryGetValue(out string? rightString))
			return string.Compare(leftString, rightString, StringComparison.Ordinal) <= 0;

		return lValue.GetNumber() <= rValue.GetNumber();
	}

	public override string ToString()
	{
		return "<=";
	}
}