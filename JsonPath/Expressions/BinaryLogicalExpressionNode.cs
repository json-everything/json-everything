using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryLogicalExpressionNode : LogicalExpressionNode
{
	public IBinaryLogicalOperator Operator { get; }
	public BooleanResultExpressionNode Left { get; }
	public BooleanResultExpressionNode Right { get; }

	public BinaryLogicalExpressionNode(IBinaryLogicalOperator op, BooleanResultExpressionNode left, BooleanResultExpressionNode right)
	{
		Operator = op;
		Left = left;
		Right = right;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}
}

internal class BinaryLogicalExpressionParser : ILogicalExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression)
	{
		var i = index;

		// parse comparison
		if (!BooleanResultExpressionParser.TryParse(source, ref i, out var left))
		{
			expression = null;
			return false;
		}

		// parse operator
		if (!BinaryLogicalOperatorParser.TryParse(source, ref i, out var op))
		{
			expression = null;
			return false;
		}

		// parse comparison
		if (!BooleanResultExpressionParser.TryParse(source, ref i, out var right))
		{
			expression = null;
			return false;
		}

		expression = new BinaryLogicalExpressionNode(op, left, right);
		index = i;
		return true;
	}
}