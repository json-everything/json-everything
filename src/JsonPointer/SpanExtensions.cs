using System;

namespace Json.Pointer;

internal static class SpanExtensions
{
	internal static char Decode(this ReadOnlySpan<char> value, ref int index)
	{
		var ch = value[index];
		if (ch == '~')
		{
			if (index + 1 >= value.Length)
				throw new PointerParseException($"Value '{value.ToString()}' does not represent a valid JSON Pointer segment");
			ch = value[index + 1] switch
			{
				'0' => '~',
				'1' => '/',
				_ => throw new PointerParseException($"Value '{value.ToString()}' does not represent a valid JSON Pointer segment")
			};
			index++;
		}

		return ch;
	}

	internal static bool TryDecode(this ReadOnlySpan<char> value, ref int index, out char ch)
	{
		ch = value[index];
		
		if (ch != '~') return true;
		if (index + 1 >= value.Length) return false;

		index++;
		switch (value[index])
		{
			case '0':
				ch = '~';
				return true;
			case '1':
				ch = '/';
				return true;
			default:
				return false;
		}
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
