using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class ValueExpressionNode
{
	public abstract JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter);

	// these operators are probably an example of what not to do.

	public static ValueExpressionNode operator +(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Add, left, right);
	}

	public static ValueExpressionNode operator -(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Subtract, left, right);
	}

	public static ValueExpressionNode operator *(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Multiply, left, right);
	}

	public static ValueExpressionNode operator /(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryValueExpressionNode(Operators.Divide, left, right);
	}

	public static ComparativeExpressionNode operator ==(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.EqualTo, left, right);
	}

	public static ComparativeExpressionNode operator !=(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.NotEqualTo, left, right);
	}

	public static ComparativeExpressionNode operator <(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.LessThan, left, right);
	}

	public static ComparativeExpressionNode operator <=(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.LessThanOrEqualTo, left, right);
	}

	public static ComparativeExpressionNode operator >(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.GreaterThan, left, right);
	}

	public static ComparativeExpressionNode operator >=(ValueExpressionNode left, ValueExpressionNode right)
	{
		return new BinaryComparativeExpressionNode(Operators.GreaterThanOrEqualTo, left, right);
	}
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
		ValueExpressionNode? value = null;
		// first get an operand
		foreach (var parser in _operandParsers)
		{
			if (parser.TryParse(source, ref index, out value)) break;
		}

		if (value is null)
		{
			expression = null;
			return false;
		}

		source.ConsumeWhitespace(ref index);

		// then use that to try to get a binary value expression
		if (!BinaryValueExpressionParser.TryParse(source, ref index, value, out expression)) 
			// if that doesn't work, just use the operand
			expression = value;

		return true;
	}
}