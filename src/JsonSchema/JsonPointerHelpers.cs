using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public static class JsonPointerHelpers
{
	public static readonly JsonPointer Wildcard = JsonPointer.Create(Guid.NewGuid().ToString("N"));
	public static readonly JsonPointer Key = JsonPointer.Create(Guid.NewGuid().ToString("N"));

	/// <summary>
	/// Evaluates a JsonPointer against a JsonElement, expanding wildcards to return all matching values and their locations.
	/// </summary>
	/// <param name="pointer">The JsonPointer to evaluate, potentially containing wildcards</param>
	/// <param name="element">The JsonElement to evaluate against</param>
	/// <returns>An array of tuples containing the matched values and their relative locations</returns>
	public static (JsonElement value, JsonPointer location)[] EvaluateWithWildcards(this JsonPointer pointer, JsonElement element)
	{
		var results = new List<(JsonElement, JsonPointer)>();
		EvaluateWithWildcardsRecursive(pointer, element, 0, JsonPointer.Empty, results);
		return results.ToArray();
	}

	private static void EvaluateWithWildcardsRecursive(JsonPointer pointer, JsonElement current, int segmentIndex, JsonPointer currentLocation, List<(JsonElement, JsonPointer)> results)
	{
		// If we've processed all segments, add the current element
		if (segmentIndex >= pointer.SegmentCount)
		{
			results.Add((current, currentLocation));
			return;
		}

		var segment = pointer.GetSegment(segmentIndex);
		var isWildcard = segment == Wildcard.GetSegment(0);

		if (isWildcard)
		{
			// Expand wildcard based on the current element type
			if (current.ValueKind == JsonValueKind.Array)
			{
				var arrayLength = current.GetArrayLength();
				for (int i = 0; i < arrayLength; i++)
				{
					var nextLocation = currentLocation.Combine(i);
					EvaluateWithWildcardsRecursive(pointer, current[i], segmentIndex + 1, nextLocation, results);
				}
			}
			else if (current.ValueKind == JsonValueKind.Object)
			{
				foreach (var property in current.EnumerateObject())
				{
					var nextLocation = currentLocation.Combine(property.Name);
					EvaluateWithWildcardsRecursive(pointer, property.Value, segmentIndex + 1, nextLocation, results);
				}
			}
			// For non-container types, wildcard doesn't match anything
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
					}
				}
				catch (FormatException)
				{
					// Not a valid array index, skip
				}
			}
			else if (current.ValueKind == JsonValueKind.Object)
			{
				var propertyName = segment.ToString();
				if (current.TryGetProperty(propertyName, out var property))
				{
					nextElement = property;
					nextLocation = currentLocation.Combine(propertyName);
				}
			}

			if (nextElement.HasValue)
			{
				EvaluateWithWildcardsRecursive(pointer, nextElement.Value, segmentIndex + 1, nextLocation, results);
			}
		}
	}
}