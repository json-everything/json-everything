using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Pointer;

internal static class SpanExtensions
{
	internal static bool TryParse(this ReadOnlySpan<char> s, out int result)
	{
		result = 0;
		if (s.IsEmpty) return false;

		// Forbid leading zeros unless it's just "0"
		if (s[0] == '0' && s.Length > 1) return false;

		// Parse digits
		long value = 0;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (c < '0' || c > '9') return false;
			
			value = value * 10 + (c - '0');
			
			// Check for overflow
			if (value > int.MaxValue) return false;
		}

		result = (int)value;
		return true;
	}

	internal static bool TryDecodeSegment(this ReadOnlySpan<char> encoded, [NotNullWhen(true)] out string? result)
	{
		var l = encoded.Length;
		if (l == 0)
		{
			result = string.Empty;
			return true;
		}

		var targetIndex = 0;
		var sourceIndex = 0;
		result = null;

		Span<char> target = l < 1024
			? stackalloc char[l]
			: new char[l];

		for (; sourceIndex < l; targetIndex++, sourceIndex++)
		{
			target[targetIndex] = encoded[sourceIndex];

			if (target[targetIndex] == '/')
			{
				return false;
			}

			if (target[targetIndex] == '~')
			{
				if (sourceIndex == l - 1)
					return false;

				if (encoded[++sourceIndex] == '0')
					continue; // we already wrote '~' so we're good

				if (encoded[sourceIndex] == '1')
					target[targetIndex] = '/';
				else
					return false; // invalid escape sequence
			}
		}

#if NET8_0_OR_GREATER
		result = new string(target[..targetIndex]);
#else
		result = target[..targetIndex].ToString();
#endif

		return true;
	}

	internal static int Encode(this ReadOnlySpan<char> key, Span<char> encoded)
	{
		var length = 0;
		foreach (var ch in key)
		{
			switch (ch)
			{
				case '~':
					encoded[length] = '~';
					encoded[length + 1] = '0';
					length += 2;
					break;
				case '/':
					encoded[length] = '~';
					encoded[length + 1] = '1';
					length += 2;
					break;
				default:
					encoded[length] = ch;
					length++;
					break;
			}
		}

		return length;
	}
}
