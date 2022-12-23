using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryLogicalExpressionNode : LogicalExpressionNode
{
	public IBinaryLogicalOperator Operator { get; }
	public BooleanResultExpressionNode Left { get; }
	public BooleanResultExpressionNode Right { get; set; }
	public int NestLevel { get; }

	public int Precedence => NestLevel * 10 + Operator.Precedence;

	public BinaryLogicalExpressionNode(IBinaryLogicalOperator op, BooleanResultExpressionNode left, BooleanResultExpressionNode right, int nestLevel)
	{
		Operator = op;
		Left = left;
		Right = right;
		NestLevel = nestLevel;
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
		int i = index;
		var nestLevel = 0;

		int Precedence(IBinaryLogicalOperator op) => nestLevel * 10 + op.Precedence;

		source.ConsumeWhitespace(ref i);

		// TODO handle !

		if (source[i] == '(')
		{
			nestLevel++;
			i++;
		}

		// first get a comparison
		if (!ComparativeExpressionParser.TryParse(source, ref i, out var comp))
		{
			expression = null;
			return false;
		}

		BooleanResultExpressionNode left = comp;

		while (i < source.Length)
		{
			var nextNest = nestLevel;
			// parse operator
			if (!BinaryLogicalOperatorParser.TryParse(source, ref i, out var op))
			{
				// if we don't get an op, then we're done
				break;
			}

			// handle (
			source.ConsumeWhitespace(ref i);
			if (source[i] == '(')
			{
				nextNest++;
				i++;
			}

			// parse right
			if (!ComparativeExpressionParser.TryParse(source, ref i, out comp))
			{
				// if we don't get a comparison, then the syntax is wrong
				expression = null;
				return false;
			}

			BooleanResultExpressionNode right = comp;

			// handle )
			source.ConsumeWhitespace(ref i);
			if (source[i] == ')')
			{
				nextNest--;
				i++;
			}

			if (left is BinaryLogicalExpressionNode bin)
			{
				if (bin.Precedence < Precedence(op))
					bin.Right = new BinaryLogicalExpressionNode(op, bin.Right, right, nestLevel);
				else
					left = new BinaryLogicalExpressionNode(op, left, right, nestLevel);
			}
			else
				left = new BinaryLogicalExpressionNode(op, left, right, nestLevel);

			nestLevel = nextNest;
		}

		if (nestLevel != 0)
		{
			expression = null;
			return false;
		}

		index = i;
		expression = left is ComparativeExpressionNode
			? new UnaryLogicalExpressionNode(Operators.NoOp, left)
			: (LogicalExpressionNode)left;
		return true;
	}
}