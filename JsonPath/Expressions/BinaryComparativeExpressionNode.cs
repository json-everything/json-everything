using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryComparativeExpressionNode : ComparativeExpressionNode
{
	public IBinaryComparativeOperator Operator { get; }
	public ValueExpressionNode Left { get; }
	public ValueExpressionNode Right { get; }

	public BinaryComparativeExpressionNode(IBinaryComparativeOperator @operator, ValueExpressionNode left, ValueExpressionNode right)
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