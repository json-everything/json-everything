using System;

namespace Json.Path;

internal static class ParsingExtensions
{
	public static void ConsumeWhitespace(this string source, ref int i)
	{
		while (i < source.Length && char.IsWhiteSpace(source[i]))
		{
			i++;
		}
	}

	public static bool TryGetInt(this string source, ref int i, out int value)
	{
		var negative = false;
		if (source[i] == '-')
		{
			negative = true;
			i++;
		}

		// Now move past digits
		var foundNumber = false;
		value = 0;
		while (i < source.Length && char.IsDigit(source[i]))
		{
			foundNumber = true;
			value = value * 10 + source[i] - '0';
			i++;
		}

		if (negative) value = -value;
		return foundNumber;
	}
}