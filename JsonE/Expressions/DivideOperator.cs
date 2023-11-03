using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class DivideOperator : IBinaryValueOperator
{
	public int Precedence => 2;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return null;

		var rNumber = rValue.GetNumber();

		return rNumber is null or 0
			? null
			: (JsonNode?)(lValue.GetNumber() / rNumber);
	}

	public override string ToString()
	{
		return "/";
	}
}