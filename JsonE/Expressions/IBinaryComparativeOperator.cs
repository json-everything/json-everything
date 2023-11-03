using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal interface IBinaryComparativeOperator : IExpressionOperator
{
	bool Evaluate(JsonNode? left, JsonNode? right);
}