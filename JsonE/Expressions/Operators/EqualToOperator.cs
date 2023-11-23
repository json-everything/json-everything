using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions.Operators;

internal class EqualToOperator : IBinaryOperator
{
	public int Precedence => 3;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is JsonValue lVal && right is JsonValue rVal &&
		    lVal.TryGetValue(out FunctionDefinition? lFunc) &&
		    rVal.TryGetValue(out FunctionDefinition? rFunc))
			return ReferenceEquals(lFunc, rFunc);

		return left.IsEquivalentTo(right);
	}

	public override string ToString()
	{
		return "==";
	}
}