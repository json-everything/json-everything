using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class UnaryLogicalExpressionNode : CompositeLogicalExpressionNode
{
	public IUnaryLogicalOperator Operator { get; }
	public LogicalExpressionNode Value { get; }

	public UnaryLogicalExpressionNode(IUnaryLogicalOperator op, LogicalExpressionNode value)
	{
		Operator = op;
		Value = value;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Value.Evaluate(globalParameter, localParameter));
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Operator is NotOperator && Value is ValueComparisonLogicalExpressionNode or BinaryLogicalExpressionNode;

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
	public bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		// currently only the "not" operator is known
		// it expects a ! then either a comparison or logical expression

		var i = index;

		if (!source.ConsumeWhitespace(ref index))
		{
			expression = null;
			return false;
		}

		// parse operator
		if (!UnaryLogicalOperatorParser.TryParse(source, ref i, out var op))
		{
			expression = null;
			return false;
		}

		// parse comparison
		if (!LogicalExpressionParser.TryParse(source, ref i, nestLevel, out var right, options))
		{
			expression = null;
			return false;
		}

		if (!source.ConsumeWhitespace(ref index))
		{
			expression = null;
			return false;
		}

		expression = new UnaryLogicalExpressionNode(op, right);
		index = i;
		return true;
	}
}