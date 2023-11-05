using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class NotOperator : IUnaryOperator
{
	public int Precedence => 20;

	public JsonNode? Evaluate(JsonNode? value)
	{
		return !value.IsTruthy();
	}

	public override string ToString()
	{
		return "!";
	}
}