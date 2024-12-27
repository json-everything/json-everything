﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Json.Path;

internal static class UtilityExtensions
{
	public static bool In<T>(this T value, params T[] range)
	{
		return range.Contains(value);
	}

#if NET9_0_OR_GREATER
	public static bool In<T>(this T value, params ReadOnlySpan<T> range) where T : IEquatable<T>
	{
		return MemoryExtensions.Contains(range, value);
	}
#endif

	public static bool In(this char value, Range range)
	{
		return range.Start.Value <= value && value < range.End.Value;
	}

	public static IEnumerable<T> Yield<T>(this T item)
	{
		yield return item;
	}

	private static bool IsValidForPropertyName(this char ch)
	{
		return ch.In('a'..('z' + 1)) ||
			   ch.In('A'..('Z' + 1)) ||
			   ch.In('0'..('9' + 1)) ||
			   ch.In('_') ||
			   ch.In(0x80..0x10FFFF);
	}

	private static bool IsValidForPropertyNameStart(this char ch)
	{
		return ch.In('a'..('z' + 1)) ||
			   ch.In('A'..('Z' + 1)) ||
			   ch.In('_') ||
			   ch.In(0x80..0x10FFFF);
	}

	public static bool TryParseName(this ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out string? name, PathParsingOptions options)
	{
		var i = index;

		if (options.TolerateExtraWhitespace && !source.ConsumeWhitespace(ref i) || i == source.Length)
		{
			name = null;
			return false;
		}

		if (i < source.Length && source[i].IsValidForPropertyNameStart())
		{
			i++;
			while (i < source.Length && source[i].IsValidForPropertyName())
			{
				i++;
			}
		}

		if (index == i)
		{
			name = null;
			return false;
		}

		name = source[index..i].ToString();
		index = i;
		return true;
	}

	public static string HandleDotNetSupportIssues(this string regex)
	{
		var sb = new StringBuilder();
		var escaped = false;
		var inBrackets = false;
		foreach (var c in regex)
		{
			if (!escaped && !inBrackets && c == '.')
			{
				// The Regex class doesn't match `.` on non-BMP unicode very well,
				// so we need to translate that to something it does understand.
				// Ref: https://github.com/ietf-wg-jsonpath/iregexp/issues/22#issuecomment-1510543510
				sb.Append(@"(?:(?![\r\n])\P{Cs}|\p{Cs}\p{Cs})");
			}
			else
			{
				sb.Append(c);
				if (c == '\\')
				{
					escaped = true;
					continue;
				}
				if (!escaped && c == '[')
				{
					inBrackets = true;
					continue;
				}

				if (!escaped && c == ']')
				{
					inBrackets = false;
					continue;
				}
			}

			escaped = false;
		}

		var dotnetTranslation = sb.ToString();
		return dotnetTranslation;
	}
}