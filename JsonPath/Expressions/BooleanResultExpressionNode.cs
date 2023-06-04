using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class BooleanResultExpressionNode : ExpressionNode, IFilterExpression
{
	public abstract bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter);
}

internal static class BooleanResultExpressionParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out BooleanResultExpressionNode? expression, PathParsingOptions options)
	{
		if (LogicalExpressionParser.TryParse(source, ref index, out var logic, options))
		{
			expression = logic;
			return true;
		}

		if (ComparativeExpressionParser.TryParse(source, ref index, out var comparison, options))
		{
			expression = comparison;
			return true;
		}

		expression = null;
		return false;
	}
}