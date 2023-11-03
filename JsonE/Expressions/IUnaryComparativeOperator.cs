using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal interface IUnaryComparativeOperator : IExpressionOperator
{
	bool Evaluate(JsonNode? value);
}