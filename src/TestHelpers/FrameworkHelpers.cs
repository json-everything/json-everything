#if NET481

using System.Text.Json.Nodes;

namespace TestHelpers;

public static class FrameworkHelpers
{
	public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
	{
		return dictionary.TryGetValue(key, out var value) ? value : default;
	}

	public static void Deconstruct(this KeyValuePair<string, JsonNode?> entry, out string key, out JsonNode? node)
	{
		key = entry.Key;
		node = entry.Value;
	}
}

#endif