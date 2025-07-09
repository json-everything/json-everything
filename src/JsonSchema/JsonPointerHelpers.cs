using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public static class JsonPointerHelpers
{
	public static readonly JsonPointer Wildcard = JsonPointer.Create(Guid.NewGuid().ToString("N"));
	public static readonly JsonPointer Key = JsonPointer.Create(Guid.NewGuid().ToString("N"));

	// Pointer cache for performance optimization
	private static readonly ConcurrentDictionary<string, JsonPointer> _stringPointerCache = new();
	private static readonly ConcurrentDictionary<int, JsonPointer> _intPointerCache = new();

	/// <summary>
	/// Gets a cached JsonPointer for the specified string segment.
	/// </summary>
	/// <param name="segment">The string segment</param>
	/// <returns>A cached JsonPointer instance</returns>
	public static JsonPointer GetCachedPointer(string segment)
	{
		return _stringPointerCache.GetOrAdd(segment, s => JsonPointer.Create(s));
	}

	/// <summary>
	/// Gets a cached JsonPointer for the specified integer segment.
	/// </summary>
	/// <param name="segment">The integer segment</param>
	/// <returns>A cached JsonPointer instance</returns>
	public static JsonPointer GetCachedPointer(int segment)
	{
		return _intPointerCache.GetOrAdd(segment, i => JsonPointer.Create(i));
	}

	/// <summary>
	/// Evaluates a JsonPointer against a JsonElement, expanding wildcards to return all matching values and their locations.
	/// </summary>
	/// <param name="pointer">The JsonPointer to evaluate, potentially containing wildcards</param>
	/// <param name="element">The JsonElement to evaluate against</param>
	/// <returns>An array of tuples containing the matched values and their relative locations</returns>
	public static (JsonElement value, JsonPointer location)[] EvaluateWithWildcards(this JsonPointer pointer, JsonElement element)
	{
		DebugWildcard(() => $"WILDCARD START: {pointer} on {element.ValueKind}");
		var results = new List<(JsonElement, JsonPointer)>();
		EvaluateWithWildcardsRecursive(pointer, element, 0, JsonPointer.Empty, results);
		DebugWildcard(() => $"WILDCARD END: {results.Count} results");
		return results.ToArray();
	}

	private static void EvaluateWithWildcardsRecursive(JsonPointer pointer, JsonElement current, int segmentIndex, JsonPointer currentLocation, List<(JsonElement, JsonPointer)> results)
	{
		// If we've processed all segments, add the current element
		if (segmentIndex >= pointer.SegmentCount)
		{
			DebugWildcard(() => $"  MATCH: {currentLocation}");
			results.Add((current, currentLocation));
			return;
		}

		var segment = pointer.GetSegment(segmentIndex);
		var isWildcard = segment == Wildcard.GetSegment(0);

		var s = segment.ToString();
		DebugWildcard(() => $"  SEGMENT[{segmentIndex}]: {s} (wildcard:{isWildcard}) on {current.ValueKind}");

		if (isWildcard)
		{
			// Expand wildcard based on the current element type
			if (current.ValueKind == JsonValueKind.Array)
			{
				var arrayLength = current.GetArrayLength();
				DebugWildcard(() => $"    ARRAY WILDCARD: expanding {arrayLength} items");
				for (int i = 0; i < arrayLength; i++)
				{
					var nextLocation = currentLocation.Combine(i);
					EvaluateWithWildcardsRecursive(pointer, current[i], segmentIndex + 1, nextLocation, results);
				}
			}
			else if (current.ValueKind == JsonValueKind.Object)
			{
				var propertyCount = current.EnumerateObject().Count();
				DebugWildcard(() => $"    OBJECT WILDCARD: expanding {propertyCount} properties");
				foreach (var property in current.EnumerateObject())
				{
					var nextLocation = currentLocation.Combine(property.Name);
					EvaluateWithWildcardsRecursive(pointer, property.Value, segmentIndex + 1, nextLocation, results);
				}
			}
			// For non-container types, wildcard doesn't match anything
			else
			{
				DebugWildcard(() => $"    WILDCARD NO MATCH: {current.ValueKind} is not a container");
			}
		}
		else
		{
			// Regular segment - try to navigate to it
			JsonElement? nextElement = null;
			var nextLocation = JsonPointer.Empty;

			if (current.ValueKind == JsonValueKind.Array)
			{
				try
				{
					var index = segment.ToInt();
					if (index >= 0 && index < current.GetArrayLength())
					{
						nextElement = current[index];
						nextLocation = currentLocation.Combine(index);
						DebugWildcard(() => $"    ARRAY ACCESS: [{index}] found");
					}
					else
					{
						DebugWildcard(() => $"    ARRAY ACCESS: [{index}] out of bounds");
					}
				}
				catch (FormatException)
				{
					var s1 = segment.ToString();
					DebugWildcard(() => $"    ARRAY ACCESS: '{s1}' not a valid index");
				}
			}
			else if (current.ValueKind == JsonValueKind.Object)
			{
				var propertyName = segment.ToString();
				if (current.TryGetProperty(propertyName, out var property))
				{
					nextElement = property;
					nextLocation = currentLocation.Combine(propertyName);
					DebugWildcard(() => $"    OBJECT ACCESS: '{propertyName}' found");
				}
				else
				{
					DebugWildcard(() => $"    OBJECT ACCESS: '{propertyName}' not found");
				}
			}
			else
			{
				DebugWildcard(() => $"    REGULAR SEGMENT: can't navigate {current.ValueKind}");
			}

			if (nextElement.HasValue)
			{
				EvaluateWithWildcardsRecursive(pointer, nextElement.Value, segmentIndex + 1, nextLocation, results);
			}
		}
	}

	[Conditional("DEBUG")]
	private static void DebugWildcard(Func<string> messageFunc)
	{
		Console.WriteLine($"[WILDCARD DEBUG] {messageFunc()}");
	}
}