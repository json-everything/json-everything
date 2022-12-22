using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions;

internal interface ILogicalExpressionParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression);
}