using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class LogicalExpressionNode : ExpressionNode, IFilterExpression
{
	public abstract bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter);
}

internal abstract class CompositeLogicalExpressionNode : LogicalExpressionNode;

internal abstract class LeafLogicalExpressionNode : LogicalExpressionNode;

internal static class LogicalExpressionParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		int i = index;

		int Precedence(IBinaryLogicalOperator op) => nestLevel * 10 + op.Precedence;

		if (!source.ConsumeWhitespace(ref i))
		{
			expression = null;
			return false;
		}

		// first get an operand
		if (!LogicalOperandParser.TryParse(source, ref i, nestLevel, out var left, options))
		{
			expression = null;
			return false;
		}

		while (i < source.Length)
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}

			// parse operator
			if (!BinaryLogicalOperatorParser.TryParse(source, ref i, out var op))
				break; // if we don't get an op, then we're done

			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}

			// parse right
			if (!LogicalOperandParser.TryParse(source, ref i, nestLevel, out var right, options))
			{
				// if we don't get a comparison, then the syntax is wrong
				expression = null;
				return false;
			}

			// assemble
			if (left is BinaryLogicalExpressionNode lBin && lBin.Precedence < Precedence(op))
				lBin.Right = new BinaryLogicalExpressionNode(lBin.Right, op, right, nestLevel);
			else
				left = new BinaryLogicalExpressionNode(left, op, right, nestLevel);
		}

		index = i;
		expression = left;
		return true;
	}
}

internal static class LogicalOperandParser
{
	private static readonly ILogicalExpressionParser[] _parsers =
	{
		new FunctionLogicalExpressionParser(),
		new ValueComparisonLogicalExpressionParser(),
		new ExistsLogicalExpressionParser(),
		new UnaryLogicalExpressionParser()
	};

	public static bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		if (source[index] == '(')
		{
			int i = index + 1;
			if (LogicalExpressionParser.TryParse(source, ref i, nestLevel + 1, out var local, options))
			{
				if (!source.ConsumeWhitespace(ref i))
				{
					expression = null;
					return false;
				}

				if (source[i] == ')')
				{
					index = i + 1;
					expression = local;
					return true;
				}
			}
		}

		foreach (var parser in _parsers)
		{
			if (parser.TryParse(source, ref index, nestLevel, out expression, options)) return true;
		}

		expression = null;
		return false;
	}
}