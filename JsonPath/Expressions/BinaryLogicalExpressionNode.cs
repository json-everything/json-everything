using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryLogicalExpressionNode : LogicalExpressionNode
{
	public IBinaryLogicalOperator Operator { get; }
	public BooleanResultExpressionNode Left { get; }
	public BooleanResultExpressionNode Right { get; }

	public BinaryLogicalExpressionNode(IBinaryLogicalOperator @operator, BooleanResultExpressionNode left, BooleanResultExpressionNode right)
	{
		Operator = @operator;
		Left = left;
		Right = right;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}
}