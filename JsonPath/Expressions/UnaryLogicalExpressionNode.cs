using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class UnaryLogicalExpressionNode : LogicalExpressionNode
{
	public IUnaryLogicalOperator Operator { get; }
	public BooleanResultExpressionNode Value { get; }

	public UnaryLogicalExpressionNode(IUnaryLogicalOperator @operator, BooleanResultExpressionNode value)
	{
		Operator = @operator;
		Value = value;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Value.Evaluate(globalParameter, localParameter));
	}
}