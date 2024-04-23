using System.Linq;
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

	/// <summary>
	/// A set of predefined single-segment JSON Pointers that contain numeric indices.
	/// </summary>
	public static readonly JsonPointer[] NumberSegments = Enumerable.Range(0, 50).Select(x => JsonPointer.Create(x)).ToArray();

}