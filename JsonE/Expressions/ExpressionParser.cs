using System;

namespace Json.JsonE.Expressions
{
	internal static class ExpressionParser
	{
		public static bool TryParse(ReadOnlySpan<char> source, ref int index, out LogicalExpressionNode? expression)
		{
			return LogicalExpressionParser.TryParse(source, ref index, out expression);
		}
	}
}
