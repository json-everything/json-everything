#if NETSTANDARD2_0
using System.Collections.Generic;

namespace Json.Schema;

internal static class DotnetCompatibility
{
	public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
	{
		return dictionary.TryGetValue(key, out var value) ? value : default;
	}

	public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key)) return false;

		dictionary.Add(key, value);
		return true;
	}
}

#endif