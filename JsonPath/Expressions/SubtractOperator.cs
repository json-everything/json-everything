using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class SubtractOperator : IBinaryValueOperator
{
	public int Precedence => 1;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			return null;

		return lValue.GetNumber() - rValue.GetNumber();
	}
}