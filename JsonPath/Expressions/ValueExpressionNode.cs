using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class ValueExpressionNode
{
	public abstract JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter);

	public static ValueExpressionNode operator +(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Add, left, right);
	}

	public static ValueExpressionNode operator -(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Subtract, left, right);
	}

	public static ValueExpressionNode operator *(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Multiply, left, right);
	}

	public static ValueExpressionNode operator /(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Divide, left, right);
	}

	public static ComparativeExpressionNode operator ==(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.EqualTo, left, right);
	}

	public static ComparativeExpressionNode operator !=(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.NotEqualTo, left, right);
	}

	public static ComparativeExpressionNode operator <(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.LessThan, left, right);
	}

	public static ComparativeExpressionNode operator <=(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.LessThanOrEqualTo, left, right);
	}

	public static ComparativeExpressionNode operator >(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.GreaterThan, left, right);
	}

	public static ComparativeExpressionNode operator >=(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.GreaterThanOrEqualTo, left, right);
	}
}