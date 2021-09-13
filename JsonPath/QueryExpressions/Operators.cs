using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.QueryExpressions
{
	internal static class Operators
	{
		public static readonly IQueryExpressionOperator Exists = new ExistsOperator();
		public static readonly IQueryExpressionOperator Not = new NotOperator();

		public static readonly IQueryExpressionOperator Multiplication = new MultiplicationOperator();
		public static readonly IQueryExpressionOperator Division = new DivisionOperator();
		public static readonly IQueryExpressionOperator Modulus = new ModulusOperator();

		public static readonly IQueryExpressionOperator Addition = new AdditionOperator();
		public static readonly IQueryExpressionOperator Subtraction = new SubtractionOperator();

		public static readonly IQueryExpressionOperator EqualTo = new EqualToOperator();
		public static readonly IQueryExpressionOperator NotEqualTo = new NotEqualToOperator();
		public static readonly IQueryExpressionOperator LessThan = new LessThanOperator();
		public static readonly IQueryExpressionOperator LessThanOrEqualTo = new LessThanOrEqualToOperator();
		public static readonly IQueryExpressionOperator GreaterThan = new GreaterThanOperator();
		public static readonly IQueryExpressionOperator GreaterThanOrEqualTo = new GreaterThanOrEqualToOperator();

		public static readonly IQueryExpressionOperator And = new AndOperator();
		public static readonly IQueryExpressionOperator Or = new OrOperator();

		public static bool TryParse(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IQueryExpressionOperator? op)
		{ 
			op = null;
			switch (span[i])
			{
				case '*':
					i++;
					op = Multiplication;
					return true;
				case '/':
					i++;
					op = Division;
					return true;
				case '%':
					i++;
					op = Modulus;
					return true;
				case '+':
					i++;
					op = Addition;
					return true;
				case '-':
					i++;
					op = Subtraction;
					return true;
				case '=':
					if (i + 1 >= span.Length) return false;
					if (span[i + 1] == '=')
					{
						i += 2;
						op = EqualTo;
						return true;
					}
					break;
				case '!':
					if (i + 1 >= span.Length) return false;
					if (span[i + 1] == '=')
					{
						i += 2;
						op = NotEqualTo;
						return true;
					}
					break;
				case '<':
					if (i + 1 < span.Length && span[i + 1] == '=')
					{
						i += 2;
						op = LessThanOrEqualTo;
						return true;
					}
					i++;
					op = LessThan;
					return true;
				case '>':
					if (i + 1 < span.Length && span[i + 1] == '=')
					{
						i += 2;
						op = GreaterThanOrEqualTo;
						return true;
					}
					i++;
					op = GreaterThan;
					return true;
				case '&':
					if (i + 1 >= span.Length) return false;
					if (span[i + 1] == '&')
					{
						i += 2;
						op = And;
						return true;
					}
					break;
				case '|':
					if (i + 1 >= span.Length) return false;
					if (span[i + 1] == '|')
					{
						i += 2;
						op = Or;
						return true;
					}
					break;
			}
			return false;
		}
	}
}