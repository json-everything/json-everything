#if NETSTANDARD2_0

using System.Collections.Generic;

namespace Json.Logic;

internal static class DeconstructionExtensions
{
	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
	{
		key = kvp.Key;
		value = kvp.Value;
	}
}

#endif