using System.Linq;

namespace Json.Patch
{
	internal class CopyOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new CopyOperationHandler();

		private CopyOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
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

			var last = operation.Path.Segments.Last();
			if (current.Object != null)
			{
				current.Object[last.Value] = from;
				return;
			}
			if (current.Array != null)
			{
				if (last.Value == "-")
				{
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

				current.Array.Insert(index, from);
				return;
			}

			context.Message = $"Path `{operation.Path}` is not present in the instance.";
		}
	}
}