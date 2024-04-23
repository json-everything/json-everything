using System;
using System.Text.Json.Nodes;

namespace Json.Pointer;

/// <summary>
/// Provides some extensions for <see cref="JsonNode"/> and family.
/// </summary>
public static class NodeExtensions
{
	/// <summary>
	/// Attempts to get a value by a JSON Pointer segment key.
	/// </summary>
	/// <param name="obj">The object to search.</param>
	/// <param name="segment">The segment value (which could contain escaped chars).</param>
	/// <param name="node">The resulting value, if found; null otherwise.</param>
	/// <returns>true if found; null otherwise.</returns>
	public static bool TryGetSegment(this JsonObject obj, ReadOnlySpan<char> segment, out JsonNode? node)
	{
		foreach (var kvp in obj)
		{
			if (!JsonPointer.SegmentEquals(segment, kvp.Key)) continue;

			node = kvp.Value;
			return true;
		}

		node = null;
		return false;
	}
}