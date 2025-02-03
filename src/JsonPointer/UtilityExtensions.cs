using System;
using System.Globalization;

namespace Json.Pointer;

internal static class UtilityExtensions
{
	public static int AsInt(this ReadOnlySpan<char> value)
	{
#if NET8_0_OR_GREATER
		return int.Parse(value, CultureInfo.InvariantCulture);
#else
		return int.Parse(value.ToString(), CultureInfo.InvariantCulture);
#endif
	}

	public static uint AsUint(this ReadOnlySpan<char> value)
	{
#if NET8_0_OR_GREATER
		return uint.Parse(value, CultureInfo.InvariantCulture);
#else
		return uint.Parse(value.ToString(), CultureInfo.InvariantCulture);
#endif
	}
}