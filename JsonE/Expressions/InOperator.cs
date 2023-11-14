using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
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
			_ => throw new BuiltInException(CommonErrors.IncorrectArgType("in"))
		};

		return values.Contains(left, JsonNodeEqualityComparer.Instance);
	}

	public override string ToString()
	{
		return " in ";
	}
}