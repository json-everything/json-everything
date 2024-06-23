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
		var left = Left.Evaluate(globalParameter, localParameter);
		var right = Right.Evaluate(globalParameter, localParameter);
		var result = Operator.Evaluate(left, right);
		return result;
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Left is BinaryLogicalExpressionNode lBin && lBin.Precedence > Operator.Precedence;

		if (useGroup)
			builder.Append('(');
		Left.BuildString(builder);
		if (useGroup)
			builder.Append(')');
		builder.Append(Operator);
		
		useGroup = Right is BinaryLogicalExpressionNode rBin && rBin.Precedence > Operator.Precedence;

		if (useGroup)
			builder.Append('(');
		Right.BuildString(builder);
		if (useGroup)
			builder.Append(')');
	}

	public override string ToString()
	{
		return $"{Left}{Operator}{Right}";
	}
}

internal class BinaryLogicalExpressionParser : ILogicalExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		int i = index;
		int originalNest = nestLevel; // need to get back to this

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
		ComparativeExpressionNode? comp = null;
		while (nestLevel >= originalNest && !ComparativeExpressionParser.TryParse(source, ref i, out comp, options))
		{
			// if we found nesting and the comparative expression parse fails,
			// pull back to include the ( and try again.
			nestLevel--;
			i--;
		}

		// if none of that worked, fail
		if (comp is null)
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
			if (source[i] == ')' && nestLevel > originalNest)
			{
				while (i < source.Length && source[i] == ')' && nestLevel > originalNest)
				{
					nestLevel--;
					i++;
				}
				if (i == source.Length)
					throw new PathParseException(i, "Unexpected end of input");
				if (nestLevel == originalNest) continue;
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
			if (!BooleanResultExpressionParser.TryParse(source, ref i, nextNest, out var right, options))
			{
				// if we don't get a comparison, then the syntax is wrong
				expression = null;
				return false;
			}

			// this logic necessarily differs from the logic in ValueExpressionParser
			// because the parser above reads an entire expression into "right",
			// whereas the ValueExpressionParser only reads the next operand.
			if (left is BinaryLogicalExpressionNode lBin && lBin.Precedence < Precedence(op))
				lBin.Right = new BinaryLogicalExpressionNode(op, lBin.Right, right, nestLevel);
			else if (right is BinaryLogicalExpressionNode rBin && rBin.Precedence <= Precedence(op))
			{
				rBin.Left = new BinaryLogicalExpressionNode(op, left, rBin.Left, nestLevel);
				left = right;
			}
			else
				left = new BinaryLogicalExpressionNode(op, left, right, nestLevel);

			nestLevel = nextNest;
		}

		if (nestLevel != originalNest)
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