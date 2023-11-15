using System;
#pragma warning disable IDE1006

namespace Json.JsonE.Expressions.Operators;

internal static class OperatorRepository
{
	// ReSharper disable InconsistentNaming
	private static readonly IBinaryOperator Add = new AddOperator();
	private static readonly IBinaryOperator Subtract = new SubtractOperator();
	private static readonly IBinaryOperator Multiply = new MultiplyOperator();
	private static readonly IBinaryOperator Divide = new DivideOperator();
	private static readonly IBinaryOperator Exponent = new ExponentOperator();

	private static readonly IBinaryOperator EqualTo = new EqualToOperator();
	private static readonly IBinaryOperator NotEqualTo = new NotEqualToOperator();
	private static readonly IBinaryOperator LessThan = new LessThanOperator();
	private static readonly IBinaryOperator LessThanOrEqualTo = new LessThanOrEqualToOperator();
	private static readonly IBinaryOperator GreaterThan = new GreaterThanOperator();
	private static readonly IBinaryOperator GreaterThanOrEqualTo = new GreaterThanOrEqualToOperator();
	private static readonly IBinaryOperator In = new InOperator();

	private static readonly IBinaryOperator And = new AndOperator();
	private static readonly IBinaryOperator Or = new OrOperator();

	private static readonly IUnaryOperator Not = new NotOperator();
	private static readonly IUnaryOperator Negate = new NegateOperator();
	private static readonly IUnaryOperator Posate = new PosateOperator();
	// ReSharper restore InconsistentNaming

	public static bool TryGetBinary(ReadOnlySpan<char> source, ref int index, out IExpressionOperator? op)
	{
		if (!source.ConsumeWhitespace(ref index))
		{
			op = null;
			return false;
		}

		switch (source[index])
		{
			case '+':
				op = Add;
				index++;
				break;
			case '-':
				op = Subtract;
				index++;
				break;
			case '/':
				op = Divide;
				index++;
				break;
			default:
				op = null;
				break;
		}

		if (op != null) return true;

		// comparative operators
		if (index > source.Length - 2)
		{
			// no need to check for < or > because there would be no room for an operand
			op = null;
			return false;
		}

		var portion = source[index..(index + 2)];

		if (portion.Equals("**".AsSpan(), StringComparison.Ordinal))
		{
			op = Exponent;
			index += 2;
		}
		else if (source[index] == '*')
		{
			op = Multiply;
			index++;
		}
		else if (portion.Equals("==".AsSpan(), StringComparison.Ordinal))
		{
			op = EqualTo;
			index += 2;
		}
		else if (portion.Equals("!=".AsSpan(), StringComparison.Ordinal))
		{
			op = NotEqualTo;
			index += 2;
		}
		else if (portion.Equals("<=".AsSpan(), StringComparison.Ordinal))
		{
			op = LessThanOrEqualTo;
			index += 2;
		}
		else if (portion.Equals(">=".AsSpan(), StringComparison.Ordinal))
		{
			op = GreaterThanOrEqualTo;
			index += 2;
		}
		else if (source[index] == '<')
		{
			op = LessThan;
			index++;
		}
		else if (source[index] == '>')
		{
			op = GreaterThan;
			index++;
		}
		else if (portion.Equals("in".AsSpan(), StringComparison.Ordinal))
		{
			op = In;
			index += 2;
		}

		// logical operators
		else if (portion.Equals("&&".AsSpan(), StringComparison.Ordinal))
		{
			op = And;
			index += 2;
		}
		else if (portion.Equals("||".AsSpan(), StringComparison.Ordinal))
		{
			op = Or;
			index += 2;
		}
		else
		{
			op = null;
			return false;
		}

		return true;
	}

	public static bool TryGetUnary(ReadOnlySpan<char> source, ref int index, out IExpressionOperator? op)
	{
		if (!source.ConsumeWhitespace(ref index))
		{
			op = null;
			return false;
		}

		switch (source[index])
		{
			case '+':
				op = Posate;
				index++;
				break;
			case '-':
				op = Negate;
				index++;
				break;
			case '!':
				op = Not;
				index++;
				break;
			default:
				op = null;
				break;
		}

		return op != null;
	}

}
