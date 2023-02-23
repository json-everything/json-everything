using System.Diagnostics.CodeAnalysis;
using System;

namespace Json.Path.Expressions;

internal abstract class LogicalExpressionNode : BooleanResultExpressionNode
{
}

internal static class LogicalExpressionParser
{
	private static readonly ILogicalExpressionParser[] _parsers =
	{
		new BinaryLogicalExpressionParser(),
		new UnaryLogicalExpressionParser(),
		new BooleanFunctionExpressionParser()
	};

	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		// TODO (efficiency opportunity) the first comparison is parsed twice
		foreach (var parser in _parsers)
		{
			if (parser.TryParse(source, ref index, out expression, options)) return true;
		}

		expression = null;
		return false;
	}
}