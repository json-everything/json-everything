using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class ExistsOperator : IUnaryComparativeOperator
{
	public bool Evaluate(JsonNode? value)
	{
		return value is not null;
	}
}