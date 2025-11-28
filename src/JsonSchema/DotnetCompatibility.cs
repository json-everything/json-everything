#if NETSTANDARD2_0
using System.Collections.Generic;

namespace Json.Schema;

internal static class DotnetCompatibility
{
	public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
	{
		return dictionary.TryGetValue(key, out var value) ? value : default;
	}
}

#endif