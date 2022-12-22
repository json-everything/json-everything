using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions;

internal interface IValueExpressionParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression);
}