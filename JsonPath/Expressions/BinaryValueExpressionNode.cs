using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryValueExpressionNode : ValueExpressionNode
{
	public IBinaryValueOperator Operator { get; }
	public ValueExpressionNode Left { get; }
	public ValueExpressionNode Right { get; }

	public BinaryValueExpressionNode(IBinaryValueOperator @operator, ValueExpressionNode left, ValueExpressionNode right)
	{
		Operator = @operator;
		Left = left;
		Right = right;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}
}