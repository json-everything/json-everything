using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class ValueExpressionNode
{
	public abstract JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter);
}

internal class ValueExpressionParser
{
	private static readonly IValueExpressionParser[] _operandParsers =
	{
		new FunctionExpressionParser(),
		new LiteralExpressionParser(),
		new PathExpressionParser(),
	};

	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression)
	{
		int i = index;
		var nestLevel = 0;

		int Precedence(IBinaryValueOperator op) => nestLevel * 10 + op.Precedence;

		source.ConsumeWhitespace(ref i);

		if (source[i] == '(')
		{
			nestLevel++;
			i++;
		}

		// first get an operand
		ValueExpressionNode? left = null;
		foreach (var parser in _operandParsers)
		{
			if (parser.TryParse(source, ref i, out left)) break;
		}

		if (left == null)
		{
			expression = null;
			return false;
		}

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
			if (!ValueOperatorParser.TryParse(source, ref i, out var op))
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
			ValueExpressionNode? right = null;
			foreach (var parser in _operandParsers)
			{
				if (parser.TryParse(source, ref i, out right)) break;
			}

			if (right == null)
			{
				// if we don't get a value, then the syntax is wrong
				expression = null;
				return false;
			}

			if (left is BinaryValueExpressionNode bin)
			{
				if (bin.Precedence < Precedence(op))
					bin.Right = new BinaryValueExpressionNode(op, bin.Right, right, nestLevel);
				else
					left = new BinaryValueExpressionNode(op, left, right, nestLevel);
			}
			else
				left = new BinaryValueExpressionNode(op, left, right, nestLevel);

			nestLevel = nextNest;
		}

		if (nestLevel != 0)
		{
			expression = null;
			return false;
		}

		index = i;
		expression = left;
		return true;
	}
}