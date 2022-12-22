using System.Diagnostics.CodeAnalysis;
using System;

namespace Json.Path.Expressions;

internal abstract class ComparativeExpressionNode : BooleanResultExpressionNode
{
}

internal class ComparativeExpressionParser
{
	private static readonly IComparativeExpressionParser[] _parsers =
	{
		new BinaryComparativeExpressionParser(),
		new UnaryComparativeExpressionParser()
	};

	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ComparativeExpressionNode? expression)
	{
		foreach (var parser in _parsers)
		{
			if (parser.TryParse(source, ref index, out expression)) return true;
		}

		expression = null;
		return false;
	}
}