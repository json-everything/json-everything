using System;
using System.Buffers;

namespace Json.Pointer;

internal static class SpanExtensions
{
	internal static string Decode(this string value)
	{
		using var memory = MemoryPool<char>.Shared.Rent();
		var span = memory.Memory.Span;
		var sourceIndex = 0;
		var spanIndex = 0;
		var sourceSpan = value.AsSpan();
		while (sourceIndex < value.Length)
		{
			span[spanIndex] = Decode(sourceSpan, ref sourceIndex);
			spanIndex++;
			sourceIndex++;
		}

		return spanIndex == sourceIndex
			? value
			: span[..spanIndex].ToString();
	}

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

	internal static ReadOnlySpan<char> Encode(this ReadOnlySpan<char> key)
	{
		using var owner = MemoryPool<char>.Shared.Rent(key.Length * 2);
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
}
