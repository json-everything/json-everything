using System.Linq;
using Json.Pointer;

namespace Json.Patch
{
	internal class MoveOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new MoveOperationHandler();

		private MoveOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			if (Equals(operation.Path, operation.From)) return;

			var current = context.Source;
			var message = EditableJsonElementHelpers.FindParentOfTarget(ref current, operation.Path);

			if (message != null)
			{
				context.Message = message;
				return;
			}

			var from = context.Source;
			message = EditableJsonElementHelpers.FindTarget(ref from, operation.From);

			if (message != null)
			{
				context.Message = message;
				return;
			}

			if (operation.Path.Segments.Length == 0)
			{
				context.Source = from;
				return;
			}

			var fromParent = context.Source;
			EditableJsonElementHelpers.FindParentOfTarget(ref fromParent, operation.From);

			var last = operation.Path.Segments.Last();
			var fromLast = operation.From.Segments.Last();
			if (current.Object != null)
			{
				context.Message = RemoveSource(fromParent, fromLast.Value, operation.From);
				if (context.Message != null) return;

				current.Object[last.Value] = from;
				return;
			}
			if (current.Array != null)
			{
				if (last.Value == "-")
				{
					context.Message = RemoveSource(fromParent, fromLast.Value, operation.From);
					if (context.Message != null) return;

					current.Array.Add(from);
					return;
				}

				if (!int.TryParse(last.Value, out var index) || 0 > index || index > current.Array.Count)
				{
					context.Message = $"Path `{operation.Path}` is not present in the instance.";
					return;
				}
				if ((index != 0 && last.Value[0] == '0') ||
				    (index == 0 && last.Value.Length > 1))
				{
					context.Message = $"Path `{operation.Path}` is not present in the instance.";
					return;
				}

				context.Message = RemoveSource(fromParent, fromLast.Value, operation.From);
				if (context.Message != null) return;

				current.Array.Insert(index, from);
				return;
			}

			context.Message = $"Path `{operation.Path}` is not present in the instance.";
		}

		private static string? RemoveSource(EditableJsonElement fromParent, string lastSegment, JsonPointer path)
		{
			if (fromParent.Object != null)
			{
				if (fromParent.Object.TryGetValue(lastSegment, out _))
				{
					fromParent.Object.Remove(lastSegment);
					return null;
				}
			}
			else if (fromParent.Array != null)
			{
				if (lastSegment == "-")
				{
					fromParent.Array.RemoveAt(fromParent.Array.Count - 1);
					return null;
				}

				if (int.TryParse(lastSegment, out var index) && -1 <= index && index < fromParent.Array.Count)
				{
					fromParent.Array.RemoveAt(index);
					return null;
				}
			}

			return $"Path `{path}` is not present in the instance.";
		}
	}
}