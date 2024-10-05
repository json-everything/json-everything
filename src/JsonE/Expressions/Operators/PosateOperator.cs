using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions.Operators;

internal class PosateOperator : IUnaryOperator
{
	public int Precedence => 20;

	public JsonNode? Evaluate(JsonNode? value)
	{
		var num = value is JsonValue val ? val.GetNumber() : null;

		return num ?? throw new InterpreterException("unary + expects number");
	}

	public override string ToString()
	{
		return "+";
	}
}