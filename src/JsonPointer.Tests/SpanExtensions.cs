using System;

namespace Json.Pointer.Tests;

internal static class SpanExtensions
{
	internal static string? Decode(this ReadOnlyMemory<char> encoded)
	{
		var l = encoded.Length;
		if (l == 0) return string.Empty;

		var targetIndex = 0;
		var sourceIndex = 0;

		Span<char> target = l < 1024
			? stackalloc char[l]
			: new char[l];

		for (; sourceIndex < l; targetIndex++, sourceIndex++)
		{
			target[targetIndex] = encoded.Span[sourceIndex];

			if (target[targetIndex] == '/')
			{
				return null;
			}

			if (target[targetIndex] == '~')
			{
				if (sourceIndex == l - 1)
					return null;

				if (encoded.Span[++sourceIndex] == '0')
					continue; // we already wrote '~' so we're good

				if (encoded.Span[sourceIndex] == '1')
					target[targetIndex] = '/';
				else
					return null; // invalid escape sequence
			}
		}

#if NET8_0_OR_GREATER
		return new string(target[..targetIndex]);
#else
		return target[..targetIndex].ToString();
#endif
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
