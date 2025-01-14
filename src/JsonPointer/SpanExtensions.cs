using System;

namespace Json.Pointer;

internal static class SpanExtensions
{
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
