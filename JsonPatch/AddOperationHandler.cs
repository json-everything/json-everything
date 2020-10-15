using System.Linq;

namespace Json.Patch
{
	internal class AddOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new AddOperationHandler();

		private AddOperationHandler()
		{
		}

		public void Process(PatchContext context, PatchOperation operation)
		{
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
				current.Object[last.Value] = new EditableJsonElement(operation.Value);
				return;
			}

			if (current.Array != null)
			{
				if (!int.TryParse(last.Value, out var index) || -1 > index || index >= current.Array.Count)
				{
					context.Message = $"Path `{operation.Path}` could not be reached.";
					return;
				}

				current.Array.Insert(index, new EditableJsonElement(operation.Value));
				return;
			}

			context.Message = $"Path `{operation.Path}` could not be reached.";
		}
	}
}