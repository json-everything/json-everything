using System;

namespace Json.JsonE.Expressions;

internal interface IComparativeExpressionParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, out ComparativeExpressionNode? expression);
}