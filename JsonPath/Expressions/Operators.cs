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

	public static readonly IBinaryLogicalOperator And = new AndOperator();
	public static readonly IBinaryLogicalOperator Or = new OrOperator();
	public static readonly IUnaryLogicalOperator Not = new NotOperator();
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

		var portion = source[index..(index + 1)];

		if (portion == "==")
		{
			op = Operators.EqualTo;
			index += 2;
		}
		else if (portion == "!=")
		{
			op = Operators.NotEqualTo;
			index += 2;
		}
		else if (portion == "<=")
		{
			op = Operators.LessThanOrEqualTo;
			index += 2;
		}
		else if (portion == ">=")
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

		var portion = source[index..(index + 1)];

		if (portion == "&&")
		{
			op = Operators.And;
			index += 2;
		}
		else if (portion == "||")
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