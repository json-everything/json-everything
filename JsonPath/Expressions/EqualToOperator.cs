using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class EqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(JsonNode? left, JsonNode? right)
	{
		return left.IsEquivalentTo(right);
	}
}