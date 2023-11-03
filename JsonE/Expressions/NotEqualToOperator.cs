using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class NotEqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(JsonNode? left, JsonNode? right)
	{
		return !left.IsEquivalentTo(right);
	}

	public override string ToString()
	{
		return "!=";
	}
}