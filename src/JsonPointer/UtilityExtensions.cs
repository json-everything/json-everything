using System;
using System.Globalization;

namespace Json.Pointer;

internal static class UtilityExtensions
{
	public static int AsInt(this ReadOnlySpan<char> value)
	{
#if NET8_0_OR_GREATER
		return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
#else
		return int.Parse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);
#endif
	}

	public static uint AsUint(this ReadOnlySpan<char> value)
	{
#if NET8_0_OR_GREATER
		return uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
#else
		return uint.Parse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);
#endif
	}
}