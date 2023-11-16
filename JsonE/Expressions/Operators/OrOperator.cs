using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Operators;

internal class OrOperator : IShortcuttingBinaryOperator
{
	public int Precedence => 2;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		return left.IsTruthy() || right.IsTruthy();
	}

	public bool ShouldContinue(JsonNode? left, out JsonNode? result)
	{
		var isTruthy = left.IsTruthy();
		result = isTruthy;
		// if result is true, then don't continue
		return !isTruthy;
	}

	public override string ToString()
	{
		return "||";
	}
}