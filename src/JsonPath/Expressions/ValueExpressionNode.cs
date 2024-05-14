using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class ValueExpressionNode : ExpressionNode
{
	public abstract PathValue? Evaluate(JsonNode? globalParameter, JsonNode? localParameter);
}

internal static class ValueExpressionParser
{
	private static readonly IValueExpressionParser[] _operandParsers =
	{
		new FunctionValueExpressionParser(),
		new LiteralExpressionParser(),
		new PathExpressionParser(),
	};

	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression, PathParsingOptions options)
	{
		int i = index;
		var nestLevel = 0;

		int Precedence(IBinaryValueOperator op) => nestLevel * 10 + op.Precedence;

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
			throw new PathParseException(i, "Unexpected end of input");

		// first get an operand
		ValueExpressionNode? left = null;
		foreach (var parser in _operandParsers)
		{
			if (parser.TryParse(source, ref i, out left, options)) break;
		}

		if (left == null)
		{
			expression = null;
			return false;
		}

		if (!options.AllowMathOperations)
		{
			index = i;
			expression = left;
			return true;
		}

		while (i < source.Length)
		{
			// handle )
			if (!source.ConsumeWhitespace(ref index))
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
			if (!ValueOperatorParser.TryParse(source, ref i, out var op))
				break; // if we don't get an op, then we're done

			// handle (
			if (!source.ConsumeWhitespace(ref index))
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
			ValueExpressionNode? right = null;
			foreach (var parser in _operandParsers)
			{
				if (parser.TryParse(source, ref i, out right, options)) break;
			}

			if (right == null)
			{
				// if we don't get a value, then the syntax is wrong
				expression = null;
				return false;
			}

			if (left is BinaryValueExpressionNode bin && bin.Precedence < Precedence(op))
				bin.Right = new BinaryValueExpressionNode(op, bin.Right, right, nestLevel);
			else
				left = new BinaryValueExpressionNode(op, left, right, nestLevel);

			nestLevel = nextNest;
		}

		if (nestLevel > 0)
		{
			expression = null;
			return false;
		}

		index = i;
		expression = left;
		return true;
	}
}
