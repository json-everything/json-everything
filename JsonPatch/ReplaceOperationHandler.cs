using System.Linq;

namespace Json.Patch
{
	internal class ReplaceOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new ReplaceOperationHandler();

		private ReplaceOperationHandler() { }

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
				if (!current.Object.TryGetValue(last.Value, out _))
				{
					context.Message = $"Path `{operation.Path}` is not present in the instance.";
					return;
				}

				current.Object[last.Value] = new EditableJsonElement(operation.Value);
				return;
			} 
			if (current.Array != null)
			{
				if (!int.TryParse(last.Value, out var index) || -1 > index || index >= current.Array.Count)
				{
					context.Message = $"Path `{operation.Path}` is not present in the instance.";
					return;
				}

				current.Array[index] = new EditableJsonElement(operation.Value);
				return;
			}

			context.Message = $"Path `{operation.Path}` is not present in the instance.";
		}
	}
}