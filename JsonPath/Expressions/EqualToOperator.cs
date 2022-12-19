using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class EqualToOperator : IBinaryComparativeOperator
{
	public bool Evaluate(JsonNode? left, JsonNode? right)
	{
		return left.IsEquivalentTo(right);
	}
}