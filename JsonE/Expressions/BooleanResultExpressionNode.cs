using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal abstract class BooleanResultExpressionNode : ExpressionNode, IFilterExpression
{
	public abstract bool Evaluate(EvaluationContext context);
}

internal static class BooleanResultExpressionParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, out BooleanResultExpressionNode? expression)
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