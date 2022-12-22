using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryValueExpressionNode : ValueExpressionNode
{
	public IBinaryValueOperator Operator { get; }
	public ValueExpressionNode Left { get; }
	public ValueExpressionNode Right { get; }

	public BinaryValueExpressionNode(IBinaryValueOperator op, ValueExpressionNode left, ValueExpressionNode right)
	{
		Operator = op;
		Left = left;
		Right = right;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}
}

internal static class BinaryValueExpressionParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, ValueExpressionNode left, [NotNullWhen(true)] out ValueExpressionNode? expression)
	{
		int i = index;

		source.ConsumeWhitespace(ref i);

		// parse operator
		if (!ValueOperatorParser.TryParse(source, ref i, out var op))
		{
			expression = null;
			return false;
		}

		source.ConsumeWhitespace(ref i);

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, out var right))
		{
			expression = null;
			return false;
		}

		// put it all together
		expression = new BinaryValueExpressionNode(op, left, right);
		index = i;
		return true;
	}
}