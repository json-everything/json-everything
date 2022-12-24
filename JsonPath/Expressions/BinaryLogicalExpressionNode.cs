using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;
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

	public override void BuildString(StringBuilder builder)
	{
		Left.BuildString(builder);
		builder.Append(Operator);
		Right.BuildString(builder);
	}

	public override string ToString()
	{
		return $"{Left}{Operator}{Right}";
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
			// handle )
			source.ConsumeWhitespace(ref i);
			if (source[i] == ')')
			{
				nestLevel--;
				i++;
				continue;
			}

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

		switch (nestLevel)
		{
			case > 0:
				expression = null;
				return false;
			case < 0:
				i--; // it can really only be -1; don't consume ) from outer expressions
				break;
		}

		index = i;
		expression = left is ComparativeExpressionNode
			? new UnaryLogicalExpressionNode(Operators.NoOp, left)
			: (LogicalExpressionNode)left;
		return true;
	}
}