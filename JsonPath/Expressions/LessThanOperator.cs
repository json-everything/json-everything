using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class LessThanOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left.TryGetSingleValue() is not JsonValue lValue ||
		    right.TryGetSingleValue() is not JsonValue rValue)
			return false;

		if (lValue.TryGetValue(out string? leftString) &&
		    rValue.TryGetValue(out string? rightString))
			return string.Compare(leftString, rightString, StringComparison.Ordinal) == -1;

		return lValue.GetNumber() < rValue.GetNumber();
	}

	public override string ToString()
	{
		return "<";
	}
}