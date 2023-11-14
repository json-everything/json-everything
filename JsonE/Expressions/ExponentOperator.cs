using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class ExponentOperator : IBinaryOperator
{
	public int Precedence => 7;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		if (left is not JsonValue lValue ||
		    right is not JsonValue rValue)
			throw new InterpreterException("infix: ** expects number ** number");

		var lNumber = (double?) lValue.GetNumber() ?? throw new InterpreterException("infix: ** expects number ** number");
		var rNumber = (double?) rValue.GetNumber() ?? throw new InterpreterException("infix: ** expects number ** number");

		return (decimal)Math.Pow(lNumber, rNumber);
	}

	public override string ToString()
	{
		return "**";
	}
}