using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class LiteralExpressionNode : ValueExpressionNode
{
	public JsonNode? Value { get; }

	public LiteralExpressionNode(JsonNode? value)
	{
		Value = value ?? JsonNull.SignalNode;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Value;
	}

	public static implicit operator LiteralExpressionNode(JsonNode? value)
	{
		return new LiteralExpressionNode(value);
	}
}