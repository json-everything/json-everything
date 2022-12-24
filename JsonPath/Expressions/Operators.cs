using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions;

internal static class Operators
{
	public static readonly IBinaryValueOperator Add = new AddOperator();
	public static readonly IBinaryValueOperator Subtract = new SubtractOperator();
	public static readonly IBinaryValueOperator Multiply = new MultiplyOperator();
	public static readonly IBinaryValueOperator Divide = new DivideOperator();

	public static readonly IBinaryComparativeOperator EqualTo = new EqualToOperator();
	public static readonly IBinaryComparativeOperator NotEqualTo = new NotEqualToOperator();
	public static readonly IBinaryComparativeOperator LessThan = new LessThanOperator();
	public static readonly IBinaryComparativeOperator LessThanOrEqualTo = new LessThanOrEqualToOperator();
	public static readonly IBinaryComparativeOperator GreaterThan = new GreaterThanOperator();
	public static readonly IBinaryComparativeOperator GreaterThanOrEqualTo = new GreaterThanOrEqualToOperator();

	public static readonly IUnaryComparativeOperator Exists = new ExistsOperator();

	public static readonly IBinaryLogicalOperator And = new AndOperator();
	public static readonly IBinaryLogicalOperator Or = new OrOperator();

	public static readonly IUnaryLogicalOperator Not = new NotOperator();
	public static readonly IUnaryLogicalOperator NoOp = new NoOpOperator();
}

internal static class OperatorParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out IExpressionOperator? op)
	{
		if (ValueOperatorParser.TryParse(source, ref index, out var valueOp))
		{
			op = valueOp;
			return true;
		}

		// unary value operator is part of value parsing, so we don't bother here.

		if (BinaryComparativeOperatorParser.TryParse(source, ref index, out var compOp))
		{
			op = compOp;
			return true;
		}

		if (BinaryLogicalOperatorParser.TryParse(source, ref index, out var binaryLogicalOp))
		{
			op = binaryLogicalOp;
			return true;
		}

		if (UnaryLogicalOperatorParser.TryParse(source, ref index, out var unaryLogicalOp))
		{
			op = unaryLogicalOp;
			return true;
		}

		op = null;
		return false;
	}
}

internal static class ValueOperatorParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out IBinaryValueOperator? op)
	{
		source.ConsumeWhitespace(ref index);

		switch (source[index])
		{
			case '+':
				op = Operators.Add;
				index++;
				break;
			case '-':
				op = Operators.Subtract;
				index++;
				break;
			case '*':
				op = Operators.Multiply;
				index++;
				break;
			case '/':
				op = Operators.Divide;
				index++;
				break;
			default:
				op = null;
				break;
		}

		return op != null;
	}
}

internal static class BinaryComparativeOperatorParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out IBinaryComparativeOperator? op)
	{
		source.ConsumeWhitespace(ref index);

		if (index > source.Length - 2)
		{
			// no need to check for < or > because there would be no room for an operand
			op = null;
			return false;
		}

		var portion = source[index..(index + 2)];

		if (portion.Equals("==", StringComparison.Ordinal))
		{
			op = Operators.EqualTo;
			index += 2;
		}
		else if (portion.Equals("!=", StringComparison.Ordinal))
		{
			op = Operators.NotEqualTo;
			index += 2;
		}
		else if (portion.Equals("<=", StringComparison.Ordinal))
		{
			op = Operators.LessThanOrEqualTo;
			index += 2;
		}
		else if (portion.Equals(">=", StringComparison.Ordinal))
		{
			op = Operators.GreaterThanOrEqualTo;
			index += 2;
		}
		else if (source[index] == '<')
		{
			op = Operators.LessThan;
			index++;
		}
		else if (source[index] == '>')
		{
			op = Operators.GreaterThan;
			index++;
		}
		else
		{
			op = null;
			return false;
		}

		return true;
	}
}

internal static class BinaryLogicalOperatorParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out IBinaryLogicalOperator? op)
	{
		source.ConsumeWhitespace(ref index);

		if (index > source.Length - 2)
		{
			op = null;
			return false;
		}

		var portion = source[index..(index + 2)];

		if (portion.Equals("&&", StringComparison.Ordinal))
		{
			op = Operators.And;
			index += 2;
		}
		else if (portion.Equals("||", StringComparison.Ordinal))
		{
			op = Operators.Or;
			index += 2;
		}
		else
		{
			op = null;
			return false;
		}

		return true;
	}
}

internal static class UnaryLogicalOperatorParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out IUnaryLogicalOperator? op)
	{
		source.ConsumeWhitespace(ref index);

		if (source[index] == '!')
		{
			op = Operators.Not;
			index++;
		}
		else
		{
			op = null;
			return false;
		}

		return true;
	}
}
