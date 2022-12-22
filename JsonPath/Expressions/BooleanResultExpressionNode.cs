using System.Diagnostics.CodeAnalysis;
using System;
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

internal class BooleanResultExpressionParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out BooleanResultExpressionNode? expression)
	{
		if (LogicalExpressionParser.TryParse(source, ref index, out var logic))
		{
			expression = logic;
			return true;
		}

		if (ComparativeExpressionParser.TryParse(source, ref index, out var comparison))
		{
			expression = comparison;
			return true;
		}

		expression = null;
		return false;
	}
}