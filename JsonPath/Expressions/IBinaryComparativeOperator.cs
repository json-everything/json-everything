using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal interface IBinaryComparativeOperator : IExpressionOperator
{
	bool Evaluate(JsonNode? left, JsonNode? right);
}