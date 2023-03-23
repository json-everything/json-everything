using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class MultiplyOperator : IBinaryValueOperator
{
	public int Precedence => 2;

	public PathValue? Evaluate(PathValue? left, PathValue? right)
	{
		if (left?.TryGetJson() is not JsonValue lValue ||
		    right?.TryGetJson() is not JsonValue rValue)
			return null;

		return (JsonNode?)(lValue.GetNumber() * rValue.GetNumber());
	}

	public override string ToString()
	{
		return "*";
	}
}