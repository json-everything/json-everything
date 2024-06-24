using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;

namespace Json.Path.Expressions;

internal abstract class ExpressionNode
{
	public abstract void BuildString(StringBuilder builder);
}

internal static class ExpressionParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		return LogicalExpressionParser.TryParse(source, ref index, 0, out expression, options);
	}
}
