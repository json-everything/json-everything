using System;
using System.Text;

using static Json.JsonE.Operators.CommonErrors;

namespace Json.JsonE.Expressions;

internal class UnaryLogicalExpressionNode : LogicalExpressionNode
{
	public IUnaryLogicalOperator Operator { get; }
	public BooleanResultExpressionNode Value { get; }

	public UnaryLogicalExpressionNode(IUnaryLogicalOperator op, BooleanResultExpressionNode value)
	{
		Operator = op;
		Value = value;
	}

	public override bool Evaluate(EvaluationContext context)
	{
		return Operator.Evaluate(Value.Evaluate(context));
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Value is BinaryComparativeExpressionNode or BinaryLogicalExpressionNode;

		builder.Append(Operator);
		if (useGroup)
			builder.Append('(');
		Value.BuildString(builder);
		if (useGroup)
			builder.Append(')');
	}

	public override string ToString()
	{
		return $"{Operator}{Value}";
	}
}

internal class UnaryLogicalExpressionParser : ILogicalExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out LogicalExpressionNode? expression)
	{
		// currently only the "not" operator is known
		// it expects a ! then either a comparison or logical expression

		var i = index;
		var nestLevel = 0;

		if (!source.ConsumeWhitespace(ref index))
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

		// parse operator
		if (!UnaryLogicalOperatorParser.TryParse(source, ref i, out var op))
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

		if (!source.ConsumeWhitespace(ref index))
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

		expression = new UnaryLogicalExpressionNode(op!, right!);
		index = i;
		return true;
	}
}