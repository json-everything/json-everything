using System.Linq;
using Json.Pointer;

namespace Json.Patch
{
	internal static class EditableJsonElementHelpers
	{
		public static string? FindParentOfTarget(ref EditableJsonElement current, JsonPointer path)
		{
			foreach (var segment in path.Segments.Take(path.Segments.Length - 1))
			{
				if (current.Object != null)
				{
					if (current.Object.TryGetValue(segment.Value, out current)) continue;
				}
				else if (current.Array != null)
				{
					if (segment.Value == "-")
					{
						current = current.Array.Last();
						continue;
					}
					if (int.TryParse(segment.Value, out var index) &&
					    0 <= index && index < current.Array.Count)
					{
						current = current.Array[index];
						continue;
					}
				}

				return $"Path `{path}` could not be reached.";
			}

			return null;
		}
		public static string? FindTarget(ref EditableJsonElement current, JsonPointer path)
		{
			foreach (var segment in path.Segments)
			{
				if (current.Object != null)
				{
					if (current.Object.TryGetValue(segment.Value, out current)) continue;
				}
				else if (current.Array != null)
				{
					if (segment.Value == "-")
					{
						current = current.Array.Last();
						continue;
					}
					if (int.TryParse(segment.Value, out var index) &&
					    0 <= index && index < current.Array.Count)
					{
						if ((index != 0 && segment.Value[0] == '0') ||
						    (index == 0 && segment.Value.Length > 1))
							return $"Path `{path}` is not present in the instance.";

						current = current.Array[index];
						continue;
					}
				}

				return $"Path `{path}` could not be reached.";
			}

			return null;
		}
	}
}