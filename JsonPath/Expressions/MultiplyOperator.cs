using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class MultiplyOperator : IBinaryValueOperator
{
	public int Precedence => 2;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left.TryGetSingleValue() is not JsonValue lValue ||
		    right.TryGetSingleValue() is not JsonValue rValue)
			return null;

		return lValue.GetNumber() * rValue.GetNumber();
	}

	public override string ToString()
	{
		return "*";
	}
}