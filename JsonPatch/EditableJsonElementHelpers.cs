using System.Linq;
using Json.Pointer;

namespace Json.Patch
{
	internal static class EditableJsonElementHelpers
	{
		public static string FindParentOfTarget(ref EditableJsonElement current, JsonPointer path)
		{
			foreach (var segment in path.Segments.Take(path.Segments.Length - 1))
			{
				if (current.Object != null)
				{
					if (current.Object.TryGetValue(segment.Value, out current)) continue;
				}
				else if (current.Array != null)
				{
					if (int.TryParse(segment.Value, out var index) &&
					    -1 <= index && index < current.Array.Count)
					{
						current = index == -1 ? current.Array.Last() : current.Array[index];
						continue;
					}
				}

				return $"Path `{path}` could not be reached.";
			}

			return null;
		}
	}
}