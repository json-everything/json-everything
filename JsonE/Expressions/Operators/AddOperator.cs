using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions.Operators;

internal class AddOperator : IBinaryOperator
{
	public int Precedence => 7;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
			right is not JsonValue rValue)
			throw new InterpreterException("infix: + expects numbers/strings + numbers/strings");

		if (lValue.TryGetValue(out string? leftString) &&
			rValue.TryGetValue(out string? rightString))
			return leftString + rightString;

		var lNumber = lValue.GetNumber() ?? throw new InterpreterException("infix: + expects numbers/strings + numbers/strings");
		var rNumber = rValue.GetNumber() ?? throw new InterpreterException("infix: + expects numbers/strings + numbers/strings");

		return lNumber + rNumber;
	}

	public override string ToString()
	{
		return "+";
	}
}