using System;
using System.Buffers;

namespace Json.Pointer;

public static class SpanExtensions
{
	public static string GetSegmentValue(this ReadOnlySpan<char> segment)
	{
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

	internal static char ConsiderEscapes(ReadOnlySpan<char> value, ref int index)
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

	internal static bool TryConsiderEscapes(ReadOnlySpan<char> value, ref int index)
	{
		var ch = value[index];
		if (ch == '~')
		{
			if (index + 1 >= value.Length) return false;
			return value[index + 1] is '0' or '1';
		}

		return true;
	}

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
		while (i < span.Length && char.IsDigit(span[i]))
		{
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