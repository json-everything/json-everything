using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Pointer;

internal static class SpanExtensions
{
#if NET9_0_OR_GREATER
	internal static bool TryParse(this ReadOnlySpan<char> s, out int result) => int.TryParse(s, out result);
#else
	internal static bool TryParse(this ReadOnlySpan<char> s, out int result)
	{
		result = 0;
		if (s.IsEmpty) return false;

		bool isNegative = false;
		int index = 0;

		// Handle sign
		if (s[0] == '-')
		{
			isNegative = true;
			index = 1;
		}
		else if (s[0] == '+')
		{
			index = 1;
		}

		// Must have at least one digit
		if (index >= s.Length) return false;

		// Parse digits
		long value = 0;
		while (index < s.Length)
		{
			char c = s[index];
			if (c < '0' || c > '9') return false;
			
			value = value * 10 + (c - '0');
			
			// Check for overflow
			if (isNegative)
			{
				if (-value < int.MinValue) return false;
			}
			else
			{
				if (value > int.MaxValue) return false;
			}
			
			index++;
		}

		result = isNegative ? -(int)value : (int)value;
		return true;
	}
#endif

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
