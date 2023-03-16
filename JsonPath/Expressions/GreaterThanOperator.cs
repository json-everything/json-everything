using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class GreaterThanOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		var jLeft = left?.TryGetJson();
		var jRight = right?.TryGetJson();

		if (jLeft is not JsonValue lValue ||
		    jRight is not JsonValue rValue)
			return false;

		if (lValue.TryGetValue(out string? leftString) &&
		    rValue.TryGetValue(out string? rightString))
			return string.Compare(leftString, rightString, StringComparison.Ordinal) == 1;

		return lValue.GetNumber() > rValue.GetNumber();
	}

	public override string ToString()
	{
		return ">";
	}
}