using System;

namespace Json.JsonE.Expressions;

internal interface IOperandExpressionParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression);
}