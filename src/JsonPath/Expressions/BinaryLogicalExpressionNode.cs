using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryLogicalExpressionNode : LogicalExpressionNode
{
	public IBinaryLogicalOperator Operator { get; }
	public BooleanResultExpressionNode Left { get; set; }
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
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		int i = index;
		var nestLevel = 0;

		int Precedence(IBinaryLogicalOperator op) => nestLevel * 10 + op.Precedence;

		if (!source.ConsumeWhitespace(ref i))
		{
			expression = null;
			return false;
		}

		while (i < source.Length && source[i] == '(')
		{
			nestLevel++;
			i++;
		}

		if (!source.ConsumeWhitespace(ref i))
		{
			expression = null;
			return false;
		}

		// first get a comparison
		if (!ComparativeExpressionParser.TryParse(source, ref i, out var comp, options))
		{
			expression = null;
			return false;
		}

		BooleanResultExpressionNode left = comp;

		while (i < source.Length)
		{
			// handle )
			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}
			if (source[i] == ')' && nestLevel > 0)
			{
				while (i < source.Length && source[i] == ')' && nestLevel > 0)
				{
					nestLevel--;
					i++;
				}
				if (i == source.Length)
					throw new PathParseException(i, "Unexpected end of input");
				if (nestLevel == 0) continue;
			}

			var nextNest = nestLevel;
			// parse operator
			if (!BinaryLogicalOperatorParser.TryParse(source, ref i, out var op))
				break; // if we don't get an op, then we're done

			// handle (
			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}
			if (source[i] == '(')
			{
				nextNest++;
				i++;
			}

			// parse right
			if (!BooleanResultExpressionParser.TryParse(source, ref i, out var right, options))
			{
				// if we don't get a comparison, then the syntax is wrong
				expression = null;
				return false;
			}

			// this logic necessarily differs from the logic in ValueExpressionParser
			// because the parser above reads an entire expression as "right",
			// whereas the ValueExpressionParser only reads the next operand.
			if (left is BinaryLogicalExpressionNode lBin && lBin.Precedence < Precedence(op))
				lBin.Right = new BinaryLogicalExpressionNode(op, lBin.Right, right, nestLevel);
			else if (right is BinaryLogicalExpressionNode rBin && Precedence(op) >= rBin.Precedence)
			{
				rBin.Left = new BinaryLogicalExpressionNode(op, left, rBin.Left, nestLevel);
				left = right;
			}
			else
				left = new BinaryLogicalExpressionNode(op, left, right, nestLevel);

			nestLevel = nextNest;
		}

		if (nestLevel > 0)
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