using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions;

internal interface IComparativeExpressionParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ComparativeExpressionNode? expression, PathParsingOptions options);
}