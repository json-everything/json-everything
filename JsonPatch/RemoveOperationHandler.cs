using System.Linq;

namespace Json.Patch
{
	internal class RemoveOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new RemoveOperationHandler();

		private RemoveOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			if (operation.Path.Segments.Length == 0)
			{
				context.Message = "Cannot remove root value.";
				return;
			}

			var current = context.Source;
			var message = EditableJsonElementHelpers.FindParentOfTarget(ref current, operation.Path);

			if (message != null)
			{
				context.Message = message;
				return;
			}

			var last = operation.Path.Segments.Last();
			if (current.Object != null)
			{
				if (!current.Object.TryGetValue(last.Value, out _))
				{
					context.Message = $"Path `{operation.Path}` is not present in the instance.";
					return;
				}

				current.Object.Remove(last.Value);
				return;
			}
			if (current.Array != null)
			{
				if (last.Value == "-")
				{
					current.Array.RemoveAt(current.Array.Count - 1);
					return;
				}

				if (!int.TryParse(last.Value, out var index) || 0 > index || index >= current.Array.Count)
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

				current.Array.RemoveAt(index);
				return;
			}

			context.Message = $"Path `{operation.Path}` is not present in the instance.";
		}
	}
}