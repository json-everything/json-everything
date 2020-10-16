using System.Linq;

namespace Json.Patch
{
	internal class ReplaceOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new ReplaceOperationHandler();

		private ReplaceOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			if (operation.Path.Segments.Length == 0)
			{
				context.Source = new EditableJsonElement(operation.Value);
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

				current.Object[last.Value] = new EditableJsonElement(operation.Value);
				return;
			} 
			if (current.Array != null)
			{
				if (last.Value == "-")
				{
					current.Array.Add(new EditableJsonElement(operation.Value));
					return;
				}

				if (!int.TryParse(last.Value, out var index) || 0 > index || index >= current.Array.Count)
				{
					context.Message = $"Path `{operation.Path}` is not present in the instance.";
					return;
				}
				if (index != 0 && last.Value[0] == '0')
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