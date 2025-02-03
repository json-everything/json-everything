using System;

namespace Json.Pointer;

internal static class UtilityExtensions
{
	public static int AsInt(this ReadOnlySpan<char> value)
	{
#if NET8_0_OR_GREATER
		return int.Parse(value);
#else
		return int.Parse(value.ToString());
#endif
	}

	public static uint AsUint(this ReadOnlySpan<char> value)
	{
#if NET8_0_OR_GREATER
		return uint.Parse(value);
#else
		return uint.Parse(value.ToString());
#endif
	}
}