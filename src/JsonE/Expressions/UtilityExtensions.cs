using System;
using System.Linq;

namespace Json.JsonE.Expressions;

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
}