using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal interface IUnaryComparativeOperator : IExpressionOperator
{
	bool Evaluate(JsonNode? value);
}