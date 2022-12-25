using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions
{
	internal static class ExpressionParser
	{
		public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression)
		{
			return LogicalExpressionParser.TryParse(source, ref index, out expression);
		}
	}
}
