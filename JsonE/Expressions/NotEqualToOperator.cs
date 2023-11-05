using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class NotEqualToOperator : IBinaryOperator
{
	public int Precedence => 3;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		return !left.IsEquivalentTo(right);
	}

	public override string ToString()
	{
		return "!=";
	}
}