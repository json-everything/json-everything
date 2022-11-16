using System.Linq;
using Json.Pointer;

namespace Json.Schema;

public static class PointerExtensions
{
	public static JsonPointer GetSibling(this JsonPointer pointer, PointerSegment newSegment)
	{
		return JsonPointer.Create(pointer.Segments.Take(pointer.Segments.Length - 1).Append(newSegment), false);
	}
}