using System;

namespace Json.JsonE.Expressions;

internal static class Operators
{
	public static readonly IBinaryOperator Add = new AddOperator();
	public static readonly IBinaryOperator Subtract = new SubtractOperator();
	public static readonly IBinaryOperator Multiply = new MultiplyOperator();
	public static readonly IBinaryOperator Divide = new DivideOperator();
	public static readonly IBinaryOperator Exponent = new ExponentOperator();

	public static readonly IBinaryOperator EqualTo = new EqualToOperator();
	public static readonly IBinaryOperator NotEqualTo = new NotEqualToOperator();
	public static readonly IBinaryOperator LessThan = new LessThanOperator();
	public static readonly IBinaryOperator LessThanOrEqualTo = new LessThanOrEqualToOperator();
	public static readonly IBinaryOperator GreaterThan = new GreaterThanOperator();
	public static readonly IBinaryOperator GreaterThanOrEqualTo = new GreaterThanOrEqualToOperator();
	public static readonly IBinaryOperator In = new InOperator();

	public static readonly IBinaryOperator And = new AndOperator();
	public static readonly IBinaryOperator Or = new OrOperator();

	public static readonly IUnaryOperator Not = new NotOperator();

	public static bool TryGet(ReadOnlySpan<char> source, ref int index, out IExpressionOperator? op)
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
			case '!':
				op = Not;
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
}
