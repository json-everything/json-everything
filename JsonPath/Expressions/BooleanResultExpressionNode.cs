using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class BooleanResultExpressionNode
{
	public abstract bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter);

	public abstract void BuildString(StringBuilder builder);
}

internal static class BooleanResultExpressionParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out BooleanResultExpressionNode? expression)
	{
		if (LogicalExpressionParser.TryParse(source, ref index, out var logic))
		{
			expression = logic;
			return true;
		}

		if (ComparativeExpressionParser.TryParse(source, ref index, out var comparison))
		{
			expression = comparison;
			return true;
		}

		expression = null;
		return false;
	}
}