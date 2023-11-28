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
}