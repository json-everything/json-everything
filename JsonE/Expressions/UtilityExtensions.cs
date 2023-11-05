using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Json.JsonE.Expressions;

internal static class UtilityExtensions
{
	public static bool In<T>(this T value, params T[] range)
	{
		return range.Contains(value);
	}

	public static bool In(this char value, Range range)
	{
		return range.Start.Value <= value && value < range.End.Value;
	}

	public static bool In(this int value, Range range)
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

	public static bool TryParseName(this ReadOnlySpan<char> source, ref int index, out string? name)
	{
		var i = index;

		if (!source.ConsumeWhitespace(ref i) || i == source.Length)
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
}