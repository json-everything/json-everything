using System;

namespace JsonPath
{
	internal static class SpanExtensions
	{
		public static bool TryGetInt(this ReadOnlySpan<char> span, ref int i, out int value)
		{
			var negative = false;
			if (span[i] == '-')
			{
				negative = true;
				i++;
			}

			// Now move past digits
			var foundNumber = false;
			value = 0;
			while (i < span.Length && char.IsDigit(span[i]))
			{
				foundNumber = true;
				value = value * 10 + span[i] - '0';
				i++;
			}

			if (negative) value = -value;
			return foundNumber;
		}
	}
}