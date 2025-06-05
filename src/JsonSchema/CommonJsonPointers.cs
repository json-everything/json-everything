using System.Collections.Concurrent;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Defines some commonly used JSON PointerOld constructs.
/// </summary>
/// <remarks>
/// These should be used whenever possible instead of allocating new instances.
/// </remarks>
public static class CommonJsonPointers
{
	/// <summary>
	/// Defines an array containing only a single empty JSON PointerOld.
	/// </summary>
	public static readonly JsonPointer_Old[] SingleEmptyPointerArray = [JsonPointer_Old.Empty];

	private static readonly ConcurrentDictionary<int, JsonPointer_Old> _numberSegments = new();

	/// <summary>
	/// A set of predefined single-segment JSON Pointers that contain numeric indices.
	/// </summary>
	public static JsonPointer_Old GetNumberSegment(int i)
	{
		return _numberSegments.GetOrAdd(i, x => JsonPointer_Old.Create(x));
	}

}