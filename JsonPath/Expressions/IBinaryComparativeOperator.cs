using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal interface IBinaryComparativeOperator
{
	bool Evaluate(JsonNode? left, JsonNode? right);
}