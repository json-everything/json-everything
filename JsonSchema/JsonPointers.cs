using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Defines some commonly used JSON Pointer constructs.
/// </summary>
/// <remarks>
/// These should be used whenever possible instead of allocating new instances.
/// </remarks>
public static class JsonPointers
{
	/// <summary>
	/// Defines an array containing only a single empty JSON Pointer.
	/// </summary>
	public static readonly JsonPointer[] SingleEmptyPointerArray = { JsonPointer.Empty };
}