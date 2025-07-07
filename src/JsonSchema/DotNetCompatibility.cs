using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Provides compatibility extension methods for .NET versions prior to .NET 8.0 and .NET 9.0.
/// </summary>
/// <remarks>This class includes methods that replicate functionality available in later versions of .NET. Use
/// these methods to ensure consistent behavior across different .NET runtime versions.</remarks>
public static class DotNetCompatibility
{
#if !NET8_0_OR_GREATER
	public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) =>
		dictionary.TryGetValue(key, out var value) ? value : defaultValue;

	public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key)) return false;

		dictionary.Add(key, value);
		return true;
	}
#endif

#if !NET9_0_OR_GREATER
	public static bool DeepEquals(this JsonElement json, JsonElement other) => json.IsEquivalentTo(other);
#else
	public static bool DeepEquals(this JsonElement json, JsonElement other) => JsonElement.DeepEquals(json, other);
#endif
}