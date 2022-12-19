using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class LessThanOperator : IBinaryComparativeOperator
{
	public bool Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return false;

		if (left.TryGetValue(out string? leftString) &&
		    left.TryGetValue(out string? rightString))
			return string.Compare(leftString, rightString, StringComparison.Ordinal) == -1;

		return lValue.GetNumber() < rValue.GetNumber();
	}
}