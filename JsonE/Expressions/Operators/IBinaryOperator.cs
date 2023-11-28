using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Operators;

internal interface IBinaryOperator : IExpressionOperator
{
	JsonNode? Evaluate(JsonNode? left, JsonNode? right);
}

internal interface IShortcuttingBinaryOperator : IBinaryOperator
{
	bool ShouldContinue(JsonNode? left, out JsonNode? result);
}