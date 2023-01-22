using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class ExistsOperator : IUnaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(JsonNode? value)
	{
		return value.TryGetValue<NodeList>(out var nodelist)
			? nodelist.Any()
			: value is not null;
	}

	public override string ToString()
	{
		return string.Empty;
	}
}