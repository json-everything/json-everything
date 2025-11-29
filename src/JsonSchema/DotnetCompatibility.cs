using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

internal static class DotnetCompatibility
{
#if !NET8_0_OR_GREATER

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

#endif

#if !NET9_0_OR_GREATER

	public static int GetPropertyCount(this JsonElement element) => element.EnumerateObject().Count();

#endif
}
