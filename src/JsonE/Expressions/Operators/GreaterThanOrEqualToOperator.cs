using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions.Operators;

internal class GreaterThanOrEqualToOperator : IBinaryOperator
{
	public int Precedence => 3;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
			right is not JsonValue rValue)
			throw new InterpreterException("infix: >= expects numbers/strings >= numbers/strings");

		if (lValue.GetValueKind() == JsonValueKind.String &&
		    rValue.GetValueKind() == JsonValueKind.String)
			return string.Compare(lValue.GetString(), rValue.GetString(), StringComparison.Ordinal) >= 0;

		var lNumber = lValue.GetNumber() ?? throw new InterpreterException("infix: >= expects numbers/strings >= numbers/strings");
		var rNumber = rValue.GetNumber() ?? throw new InterpreterException("infix: >= expects numbers/strings >= numbers/strings");

		return lNumber >= rNumber;
	}

	public override string ToString()
	{
		return ">=";
	}
}