using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions
{
	internal static class ExpressionParser
	{
		private enum State
		{
			Logic,
			Comparison,
			Value
		}

		// different things we might encounter
		//   existence: @.foo
		//     value (wrapped in an existsOp) (do we need a no-op logical unary?)
		//   standard expression: @.foo == 5
		//     value compOp value (yeah, may need that no-op logical unary)
		//   logical expressions: @.foo == 5 && @.bar == 6
		//     value compOp value logicOp value compOp value
		//   non-existence: !@.foo
		//     notLogicOp value (wrap value in existsOp)

		public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out LogicalExpressionNode? expression)
		{
			var i = index;
			var nestLevel = 0;

			while (i < source.Length && nestLevel >= 0)
			{
				if (source[i] == '(')
				{
					nestLevel++;
					i++;
				}
				else if (source[i] == ')')
				{
					nestLevel--;
					i++;
				}
				else
				{
					// we might get a 'not' first, so we need to check for that
					OperatorParser.TryParse(source, ref i, out var op);
					if (!ValueExpressionParser.TryParse(source, ref i, out var value))
					{
						expression = null;
						return false;
					}
				}
				i++;
			}

			if (i == source.Length || nestLevel < 0)
			{
				expression = null;
				return false;
			}

			index = i;

			// TODO actually return something
			expression = null;
			return false;
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
