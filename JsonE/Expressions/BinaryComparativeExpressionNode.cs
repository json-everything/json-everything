using System;
using System.Text;

using static Json.JsonE.Operators.CommonErrors;

namespace Json.JsonE.Expressions;

internal class BinaryComparativeExpressionNode : ComparativeExpressionNode
{
	public IBinaryComparativeOperator Operator { get; }
	public ValueExpressionNode Left { get; }
	public ValueExpressionNode Right { get; }

	public BinaryComparativeExpressionNode(IBinaryComparativeOperator op, ValueExpressionNode left, ValueExpressionNode right)
	{
		Operator = op;
		Left = left;
		Right = right;
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

internal class BinaryComparativeExpressionParser : IComparativeExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ComparativeExpressionNode? expression)
	{
		var i = index;
		var nestLevel = 0;

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
		if (i == source.Length)
			throw new TemplateException(EndOfInput(i));

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, out var left))
		{
			expression = null;
			return false;
		}

		// parse operator
		if (!BinaryComparativeOperatorParser.TryParse(source, ref i, out var op))
		{
			expression = null;
			return false;
		}

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, out var right))
		{
			expression = null;
			return false;
		}

		if (!source.ConsumeWhitespace(ref i))
		{
			expression = null;
			return false;
		}
		while (i < source.Length && source[i] == ')' && nestLevel > 0)
		{
			nestLevel--;
			i++;
		}
		if (i == source.Length)
			throw new TemplateException(EndOfInput(i));
		if (nestLevel != 0)
		{
			expression = null;
			return false;
		}

		expression = new BinaryComparativeExpressionNode(op!, left!, right!);
		index = i;
		return true;
	}
}