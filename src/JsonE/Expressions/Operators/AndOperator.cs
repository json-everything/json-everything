using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Operators;

internal class AndOperator : IShortcuttingBinaryOperator
{
	public int Precedence => 1;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		return left.IsTruthy() && right.IsTruthy();
	}

	public bool ShouldContinue(JsonNode? left, out JsonNode? result)
	{
		var isTruthy = left.IsTruthy();
		result = isTruthy;
		// if result is false, then don't continue
		return isTruthy;
	}

	public override string ToString()
	{
		return "&&";
	}
}