using System;
using System.Collections.Generic;
using System.Linq;
using Json.Path.QueryExpressions;

namespace Json.Path
{
	internal static class SpanExtensions
	{
		public static void ConsumeWhitespace(this ReadOnlySpan<char> span, ref int i)
		{
			while (i < span.Length && char.IsWhiteSpace(span[i]))
			{
				i++;
			}
		}

		public static bool TryGetInt(this ReadOnlySpan<char> span, ref int i, out int value)
		{
			var negative = false;
			if (span[i] == '-')
			{
				negative = true;
				i++;
			}

			// Now move past digits
			var foundNumber = false;
			value = 0;
			while (i < span.Length && char.IsDigit(span[i]))
			{
				foundNumber = true;
				value = value * 10 + span[i] - '0';
				i++;
			}

			if (negative) value = -value;
			return foundNumber;
		}

		// expects the full expression, including the ()
		public static bool TryParseExpression(this ReadOnlySpan<char> span, ref int i, out QueryExpressionNode expression)
		{
			var index = i;
			if (span[index] != '(')
			{
				expression = null;
				return false;
			}

			index++;
			if (!QueryExpressionNode.TryParseSingle(span, ref index, out var left))
			{
				expression = null;
				return false;
			}

			var followingNodes = new List<(IQueryExpressionOperator, QueryExpressionNode)>();
			while (index < span.Length && span[index] != ')')
			{
				if (!Operators.TryParse(span, ref index, out var op))
				{
					expression = null;
					return false;
				}

				QueryExpressionNode right;
				if (span[index] == '(')
				{
					if (!span.TryParseExpression(ref index, out right))
					{
						expression = null;
						return false;
					}
				}
				else if (!QueryExpressionNode.TryParseSingle(span, ref index, out right))
				{
					expression = null;
					return false;
				}
				followingNodes.Add((op, right));
			}

			if (!followingNodes.Any())
			{
				i = index;
				expression = new QueryExpressionNode(left, Operators.Exists, null);
				return true;
			}

			var current = new Stack<QueryExpressionNode>();
			QueryExpressionNode root = null;
			foreach (var (op, node) in followingNodes)
			{
				if (root == null)
				{
					root = new QueryExpressionNode(left, op, node);
					current.Push(root);
					continue;
				}

				while (current.Any() && current.Peek().Operator.OrderOfOperation < op.OrderOfOperation)
				{
					current.Pop();
				}

				if (current.Any())
				{
					current.Peek().InsertRight(op, node);
					continue;
				}

				root = new QueryExpressionNode(root, op, node);
				current.Push(root);
			}

			i = index;
			expression = root;
			return true;
		}
	}
}