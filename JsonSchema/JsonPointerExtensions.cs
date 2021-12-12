using System.Linq;
using Json.Pointer;

namespace Json.Schema
{
	internal static class JsonPointerExtensions
	{
		public static bool StartsWith(this JsonPointer pointer, JsonPointer other)
		{
			if (!other.Segments.Any()) return true;
			if (pointer.Segments.Length < other.Segments.Length) return false;
			return !other.Segments.Where((t, i) => pointer.Segments[i] != t).Any();
		}
	}
}