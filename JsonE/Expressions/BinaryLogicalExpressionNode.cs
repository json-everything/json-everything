using System;
using System.Text;

using static Json.JsonE.Operators.CommonErrors;

namespace Json.JsonE.Expressions;

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

	public override bool Evaluate(EvaluationContext context)
	{
		return Operator.Evaluate(Left.Evaluate(context), Right.Evaluate(context));
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
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out LogicalExpressionNode? expression)
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
		if (!ComparativeExpressionParser.TryParse(source, ref i, out var comp))
		{
			expression = null;
			return false;
		}

		BooleanResultExpressionNode left = comp!;

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
					throw new TemplateException(EndOfInput(i));
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
			if (!BooleanResultExpressionParser.TryParse(source, ref i, out var right))
			{
				// if we don't get a comparison, then the syntax is wrong
				expression = null;
				return false;
			}

			if (left is BinaryLogicalExpressionNode bin)
			{
				if (bin.Precedence < Precedence(op!))
					bin.Right = new BinaryLogicalExpressionNode(op!, bin.Right, right!, nestLevel);
				else
					left = new BinaryLogicalExpressionNode(op!, left, right!, nestLevel);
			}
			else
				left = new BinaryLogicalExpressionNode(op!, left, right!, nestLevel);

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