using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Operators;

internal class AndOperator : IBinaryOperator
{
	public int Precedence => 1;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		return left.IsTruthy() && right.IsTruthy();
	}

	public override string ToString()
	{
		return "&&";
	}
}