using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Json.Pointer;

/// <summary>
/// More extensions on <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
	/// <summary>
	/// Finds an item in a dictionary that matches a pointer segment without having to get the segment's value as a string.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="dictionary">The dictionary.</param>
	/// <param name="pointerSegment">The pointer segment.</param>
	/// <param name="value">The value if found; default otherwise.</param>
	/// <returns>true if found; false otherwise.</returns>
	public static bool TryMatchPointerSegment<T>(this IReadOnlyDictionary<string, T> dictionary, ReadOnlySpan<char> pointerSegment, [NotNullWhen(true)] out T? value)
	{
		foreach (var kvp in dictionary)
		{
			if (pointerSegment.SegmentEquals(kvp.Key))
			{
				value = kvp.Value!;
				return true;
			}
		}

		value = default;
		return false;
	}
}