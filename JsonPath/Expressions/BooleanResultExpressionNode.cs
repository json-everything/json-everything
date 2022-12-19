using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class BooleanResultExpressionNode
{
	public abstract bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter);

	public static BooleanResultExpressionNode operator &(BooleanResultExpressionNode left, BooleanResultExpressionNode right)
	{
		return new BinaryLogicalExpressionNode(Operators.And, left, right);
	}

	public static BooleanResultExpressionNode operator |(BooleanResultExpressionNode left, BooleanResultExpressionNode right)
	{
		return new BinaryLogicalExpressionNode(Operators.Or, left, right);
	}

	public static BooleanResultExpressionNode operator !(BooleanResultExpressionNode value)
	{
		return new UnaryLogicalExpressionNode(Operators.Not, value);
	}
}