using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path;

internal static class SpanExtensions
{
	public static void ConsumeWhitespace(this ReadOnlySpan<char> span, ref int i)
	{
		while (i < span.Length && char.IsWhiteSpace(span[i]))
		{
			i++;
		}
		if (i == span.Length)
			throw new PathParseException(i, "Unexpected end of input");
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

	public static bool TryParseJson(this ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out JsonNode? node)
	{
		span.ConsumeWhitespace(ref i);

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
				case '.':
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					end = i;
					var allowDash = true;
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
					node = default;
					return false;
			}

			var block = span[i..end];
			if (block[0] == '\'' && block[^1] == '\'')
				block = $"\"{block[1..^1].ToString()}\"".AsSpan();
			node = JsonNode.Parse(block.ToString()) ?? JsonNull.SignalNode;
			i = end;
			return true;
		}
		catch
		{
			node = default;
			return false;
		}
	}

	public static bool TryParseName(this ReadOnlySpan<char> source, ref int index, List<ISelector> selectors)
	{
		var i = index;

		source.ConsumeWhitespace(ref i);

		while (i < source.Length && source[i].IsValidForPropertyName())
		{
			i++;
		}

		if (index == i) return false;

		var name = source[index..i].ToString();
		selectors.Add(new NameSelector(name));
		index = i;
		return true;
	}
}