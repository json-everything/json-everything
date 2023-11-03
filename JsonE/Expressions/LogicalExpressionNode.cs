using System;

namespace Json.JsonE.Expressions;

internal abstract class LogicalExpressionNode : BooleanResultExpressionNode
{
}

internal static class LogicalExpressionParser
{
	private static readonly ILogicalExpressionParser[] _parsers =
	{
		new BinaryLogicalExpressionParser(),
		new UnaryLogicalExpressionParser(),
		new FunctionBooleanExpressionParser()
	};

	public static bool TryParse(ReadOnlySpan<char> source, ref int index, out LogicalExpressionNode? expression)
	{
		// TODO (efficiency opportunity) the first comparison is parsed twice
		foreach (var parser in _parsers)
		{
			if (parser.TryParse(source, ref index, out expression)) return true;
		}

		expression = null;
		return false;
	}
}