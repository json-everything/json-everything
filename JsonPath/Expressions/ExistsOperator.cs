using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class ExistsOperator : IUnaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(JsonNode? value)
	{
		if (value is not JsonValue jValue)
			return value is not null;

		if (jValue.TryGetValue(out NodeList? nodeList))
			return nodeList.Any();

		return true;
	}

	public override string ToString()
	{
		return string.Empty;
	}
}