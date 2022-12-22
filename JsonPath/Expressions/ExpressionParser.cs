using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions
{
	internal static class ExpressionParser
	{
		public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression)
		{
			//if (source[index] == '(')
			//{
			//	var start = index;
			//	var nestLevel = 1;
			//	index++;
			//	while (index < source.Length && nestLevel != 0)
			//	{
			//		if (source[index] == '(')
			//			nestLevel++;
			//		else if (source[index] == ')')
			//			nestLevel--;
			//	}
			//	if (index == source.Length)
			//		throw new PathParseException(index, "Unexpected end of input");

			//	var end = index - 1;

			//}

			return LogicalExpressionParser.TryParse(source, ref index, out expression);
		}

		//public static bool TryParseExpression(this ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out BooleanResultExpressionNode? expression)
		//{
		//	if (span[i] != '(')
		//	{
		//		expression = null;
		//		return false;
		//	}

		//	i++;
		//	span.ConsumeWhitespace(ref i);
		//	if (!QueryExpressionNode.TryParseSingleValue(span, ref i, out var left))
		//	{
		//		expression = null;
		//		return false;
		//	}

		//	var followingNodes = new List<(IQueryExpressionOperator, QueryExpressionNode)>();
		//	while (i < span.Length && span[i] != ')')
		//	{
		//		span.ConsumeWhitespace(ref i);
		//		if (!Operators.TryParse(span, ref i, out var op))
		//		{
		//			expression = null;
		//			return false;
		//		}

		//		QueryExpressionNode? right;
		//		span.ConsumeWhitespace(ref i);
		//		if (span[i] == '(')
		//		{
		//			span.ConsumeWhitespace(ref i);
		//			if (!span.TryParseExpression(ref i, out right))
		//			{
		//				expression = null;
		//				return false;
		//			}
		//		}
		//		else
		//		{
		//			span.ConsumeWhitespace(ref i);
		//			if (!QueryExpressionNode.TryParseSingleValue(span, ref i, out right))
		//			{
		//				expression = null;
		//				return false;
		//			}
		//		}

		//		followingNodes.Add((op, right));
		//	}

		//	i++; // consume ')'

		//	if (!followingNodes.Any())
		//	{
		//		expression = left.Operator is NotOperator
		//			? left
		//			: new QueryExpressionNode(left, Operators.Exists, null!);
		//		return true;
		//	}

		//	var current = new Stack<QueryExpressionNode>();
		//	QueryExpressionNode? root = null;
		//	foreach (var (op, node) in followingNodes)
		//	{
		//		if (root == null)
		//		{
		//			root = new QueryExpressionNode(left, op, node);
		//			current.Push(root);
		//			continue;
		//		}

		//		while (current.Any() && current.Peek().Operator?.OrderOfOperation < op.OrderOfOperation)
		//		{
		//			current.Pop();
		//		}

		//		if (current.Any())
		//		{
		//			current.Peek().InsertRight(op, node);
		//			continue;
		//		}

		//		root = new QueryExpressionNode(root, op, node);
		//		current.Push(root);
		//	}

		//	expression = root;
		//	return expression != null;
		//}
	}
}
