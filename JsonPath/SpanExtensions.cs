using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
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
		public static bool TryParseExpression(this ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out QueryExpressionNode? expression)
		{
			if (span[i] != '(')
			{
				expression = null;
				return false;
			}

			i++;
			span.ConsumeWhitespace(ref i);
			if (!QueryExpressionNode.TryParseSingleValue(span, ref i, out var left))
			{
				expression = null;
				return false;
			}

			var followingNodes = new List<(IQueryExpressionOperator, QueryExpressionNode)>();
			while (i < span.Length && span[i] != ')')
			{
				span.ConsumeWhitespace(ref i);
				if (!Operators.TryParse(span, ref i, out var op))
				{
					expression = null;
					return false;
				}

				QueryExpressionNode? right;
				span.ConsumeWhitespace(ref i);
				if (span[i] == '(')
				{
					span.ConsumeWhitespace(ref i);
					if (!span.TryParseExpression(ref i, out right))
					{
						expression = null;
						return false;
					}
				}
				else
				{
					span.ConsumeWhitespace(ref i);
					if (!QueryExpressionNode.TryParseSingleValue(span, ref i, out right))
					{
						expression = null;
						return false;
					}
				}

				followingNodes.Add((op, right));
			}

			i++; // consume ')'

			if (!followingNodes.Any())
			{
				expression = left.Operator is NotOperator
					? left
					: new QueryExpressionNode(left, Operators.Exists, null!);
				return true;
			}

			var current = new Stack<QueryExpressionNode>();
			QueryExpressionNode? root = null;
			foreach (var (op, node) in followingNodes)
			{
				if (root == null)
				{
					root = new QueryExpressionNode(left, op, node);
					current.Push(root);
					continue;
				}

				while (current.Any() && current.Peek().Operator?.OrderOfOperation < op.OrderOfOperation)
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

			expression = root;
			return expression != null;
		}

		public static bool TryParseJsonElement(this ReadOnlySpan<char> span, ref int i, out JsonElement element)
		{
			try
			{
				int end = i;
				char endChar;
				switch (span[i])
				{
					case 'f':
						end += 5;
						break;
					case 't':
					case 'n':
						end += 4;
						break;
					case '.': case '-': case '0':
					case '1': case '2': case '3':
					case '4': case '5': case '6':
					case '7': case '8': case '9':
						end = i;
						var allowDash = false;
						while (end < span.Length && (span[end].In('0'..('9' + 1)) ||
						                             span[end].In('e', '.', '-')))
						{
							if (!allowDash && span[end] == '-') break;
							allowDash = span[end] == 'e';
							end++;
						}
						break;
					case '\'':
					case '"':
						end = i + 1;
						endChar = span[i];
						while (end < span.Length && span[end] != endChar)
						{
							if (span[end] == '\\')
							{
								end++;
								if (end >= span.Length) break;
							}
							end++;
						}

						end++;
						break;
					case '{':
					case '[':
						end = i + 1;
						endChar = span[i] == '{' ? '}' : ']';
						var inString = false;
						while (end < span.Length)
						{
							var escaped = false;
							if (span[end] == '\\')
							{
								escaped = true;
								end++;
								if (end >= span.Length) break;
							}
							if (!escaped && span[end] == '"')
							{
								inString = !inString;
							}
							else if (!inString && span[end] == endChar) break;

							end++;
						}

						end++;
						break;
					default:
						element = default;
						return false;
				}
				
				var block = span[i..end];
				if (block[0] == '\'' && block[^1] == '\'')
					block = $"\"{block[1..^1].ToString()}\"".AsSpan();
				using var doc = JsonDocument.Parse(block.ToString());
				element = doc.RootElement.Clone();
				i = end;
				return true;
			}
			catch
			{
				element = default;
				return false;
			}
		}
	}
}