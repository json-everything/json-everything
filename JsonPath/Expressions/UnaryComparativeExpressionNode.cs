using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class UnaryComparativeExpressionNode : ComparativeExpressionNode
{
	public IUnaryComparativeOperator Operator { get; }
	public ValueExpressionNode Value { get; }

	public UnaryComparativeExpressionNode(IUnaryComparativeOperator @operator, ValueExpressionNode value)
	{
		Operator = @operator;
		Value = value;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Value.Evaluate(globalParameter, localParameter));
	}
}