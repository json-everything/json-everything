using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Operators;

internal interface IUnaryOperator : IExpressionOperator
{
	JsonNode? Evaluate(JsonNode? node);
}