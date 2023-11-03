using System;

namespace Json.JsonE.Expressions;

internal interface IValueExpressionParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, out ValueExpressionNode? expression);
}