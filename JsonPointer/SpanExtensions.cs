using System;
using System.Buffers;

namespace Json.Pointer;

/// <summary>
/// Provides some extensions on <see cref="Span{Char}"/>.
/// </summary>
public static class SpanExtensions
{
	/// <summary>
	/// Decodes a pointer segment string, considering escapes.
	/// </summary>
	/// <param name="segment">The segment.</param>
	/// <returns>The decoded string.</returns>
	/// <exception cref="PointerParseException">Thrown when the span does not represent a valid JSON Pointer segment.</exception>
	public static string GetSegmentName(this ReadOnlySpan<char> segment)
	{
		if (segment.Length == 0) return string.Empty;

		using var owner = MemoryPool<char>.Shared.Rent(segment.Length);
		var span = owner.Memory.Span;
		var j = 0;
		for (int i = 0; i < segment.Length; i++, j++)
		{
			if (segment[i] == '~')
			{
				if (i + 1 >= segment.Length)
					throw new PointerParseException("Value does not represent a valid JSON Pointer segment");
				span[j] = segment[i + 1] switch
				{
					'0' => '~',
					'1' => '/',
					_ => throw new PointerParseException("Value does not represent a valid JSON Pointer segment")
				};
				i++;
				continue;
			}

			span[j] = segment[i];
		}

		return span[..j].ToString();
	}

	/// <summary>
	/// Attempts to get an integer index from a pointer segment.
	/// </summary>
	/// <param name="segment">The segment.</param>
	/// <returns>The index value.</returns>
	/// <exception cref="PointerParseException">Thrown when the span does not represent a valid integer.</exception>
	public static int GetSegmentIndex(this ReadOnlySpan<char> segment)
	{
		return segment.TryGetInt(out var value) ? value : throw new PointerParseException("Value does not represent an index");
	}

	/// <summary>
	/// Compares a pointer segment to an expected string.
	/// </summary>
	/// <param name="segment">The pointer segment.</param>
	/// <param name="expected">The expected string.</param>
	/// <returns>true if the segment and the string are equivalent; false otherwise.</returns>
	/// <remarks>
	/// The escapes for `~` and `/` are considered for the pointer segment, but not the expected string.
	/// </remarks>
	public static bool SegmentEquals(this ReadOnlySpan<char> segment, string expected)
	{
		if (string.IsNullOrEmpty(expected)) return segment.Length == 0;
		if (segment.Length == 0) return false;

		var aIndex = 0;
		var bIndex = 0;

		while (aIndex < segment.Length && bIndex < expected.Length)
		{
			var aChar = segment.Decode(ref aIndex);
			var bChar = expected[bIndex];

			if (aChar != bChar) return false;

			aIndex++;
			bIndex++;
		}

		return aIndex == segment.Length && bIndex == expected.Length;
	}

	internal static char Decode(this ReadOnlySpan<char> value, ref int index)
	{
		var ch = value[index];
		if (ch == '~')
		{
			if (index + 1 >= value.Length)
				throw new PointerParseException("Value does not represent a valid JSON Pointer segment");
			ch = value[index + 1] switch
			{
				'0' => '~',
				'1' => '/',
				_ => throw new PointerParseException("Value does not represent a valid JSON Pointer segment")
			};
			index++;
		}

		return ch;
	}

	internal static bool TryDecode(this ReadOnlySpan<char> value, ref int index)
	{
		var ch = value[index];
		if (ch == '~')
		{
			if (index + 1 >= value.Length) return false;
			return value[index + 1] is '0' or '1';
		}

		return true;
	}

	internal static ReadOnlySpan<char> Encode(this ReadOnlySpan<char> key)
	{
		var owner = MemoryPool<char>.Shared.Rent(key.Length * 2);
		var span = owner.Memory.Span;

		var length = 0;
		foreach (var ch in key)
		{
			switch (ch)
			{
				case '~':
					span[length] = '~';
					span[length + 1] = '0';
					length += 2;
					break;
				case '/':
					span[length] = '~';
					span[length + 1] = '1';
					length += 2;
					break;
				default:
					span[length] = ch;
					length++;
					break;
			}
		}

		return span[..length];
	}

	/// <summary>
	/// Attempts to parse an integer from the span.  Included for .Net Standard 2.0 support.
	/// </summary>
	/// <param name="span">The span.</param>
	/// <param name="value">The value if successful; 0 otherwise.</param>
	/// <returns>true if successful; false otherwise.</returns>
	public static bool TryGetInt(this ReadOnlySpan<char> span, out int value)
	{
#if NETSTANDARD2_0
		var negative = false;
		var i = 0;
		if (span[i] == '-')
		{
			negative = true;
			i++;
		}

		// Now move past digits
		var foundNumber = false;
		var zeroStart = false;
		long parsedValue = 0;
		var overflowed = false;
		value = 0;
		while (i < span.Length && span[i] != '/')
		{
			if (!char.IsDigit(span[i])) return false;
			if (zeroStart) return false;

			if (!foundNumber && span[i] == '0')
				zeroStart = true;

			foundNumber = true;
			if (!overflowed)
			{
				parsedValue = parsedValue * 10 + span[i] - '0';
				overflowed = parsedValue is <= -2L << 53 or >= 2L << 53;
			}

			i++;
		}

		if (overflowed) return false;

		if (negative) parsedValue = -parsedValue;

		value = (int)Math.Min(int.MaxValue, Math.Max(int.MinValue, parsedValue));
		return foundNumber;
#else
		return int.TryParse(span, out value);
#endif
	}
}