using System;

namespace Json.JsonE.Expressions;

internal interface ILogicalExpressionParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, out LogicalExpressionNode? expression);
}