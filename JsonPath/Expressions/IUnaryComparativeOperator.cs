using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal interface IUnaryComparativeOperator
{
	bool Evaluate(JsonNode? value);
}