using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class OrOperator : IBinaryOperator
{
	public int Precedence => 2;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		return left.IsTruthy() || right.IsTruthy();
	}

	public override string ToString()
	{
		return "||";
	}
}