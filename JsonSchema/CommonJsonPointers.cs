using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Defines some commonly used JSON Pointer constructs.
/// </summary>
/// <remarks>
/// These should be used whenever possible instead of allocating new instances.
/// </remarks>
public static class CommonJsonPointers
{
	/// <summary>
	/// Defines an array containing only a single empty JSON Pointer.
	/// </summary>
	public static readonly JsonPointer[] SingleEmptyPointerArray = { JsonPointer.Empty };

	private static readonly Dictionary<int, JsonPointer> _numberSegments = new();

	/// <summary>
	/// A set of predefined single-segment JSON Pointers that contain numeric indices.
	/// </summary>
	public static JsonPointer GetNumberSegment(int i)
	{
		if (!_numberSegments.TryGetValue(i, out var pointer))
		{
			_numberSegments[i] = pointer = JsonPointer.Create(i);
		}

		return pointer;
	}

}