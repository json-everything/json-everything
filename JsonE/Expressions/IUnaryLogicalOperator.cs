using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal interface IUnaryOperator : IExpressionOperator
{
	JsonNode? Evaluate(JsonNode? node);
}