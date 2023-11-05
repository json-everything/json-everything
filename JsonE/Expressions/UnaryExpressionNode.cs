using System;
using System.Text;
using System.Text.Json.Nodes;
using static Json.JsonE.Operators.CommonErrors;

namespace Json.JsonE.Expressions;

internal class UnaryExpressionNode : ExpressionNode
{
	public IUnaryOperator Operator { get; }
	public ExpressionNode Value { get; }

	public UnaryExpressionNode(IUnaryOperator op, ExpressionNode value)
	{
		Operator = op;
		Value = value;
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		return Operator.Evaluate(Value.Evaluate(context));
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Value is BinaryExpressionNode;

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

internal class UnaryExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression)
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
		if (!Operators.TryGet(source, ref i, out var op) || op is not IUnaryOperator unOp)
		{
			expression = null;
			return false;
		}

		// parse comparison
		if (!ExpressionParser.TryParse(source, ref i, out var right))
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

		expression = new UnaryExpressionNode(unOp, right!);
		index = i;
		return true;
	}
}