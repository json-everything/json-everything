using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class InOperator : IBinaryOperator
{
	public int Precedence => 4;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		var values = right switch
		{
			JsonArray arr => arr,
			JsonObject obj => obj.Select(x => x.Value),
			_ => null
		};

		if (values == null) return false;

		return values.Contains(left, JsonNodeEqualityComparer.Instance);
	}

	public override string ToString()
	{
		return " in ";
	}
}