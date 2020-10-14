using System.Linq;

namespace Json.Patch
{
	internal class AddOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new AddOperationHandler();

		private AddOperationHandler(){}

		public void Process(PatchContext context, PatchOperation operation)
		{
			var current = context.Source;

			foreach (var segment in operation.Path.Segments.Take(operation.Path.Segments.Length - 1))
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

				context.Message = $"Path `{operation.Path}` could not be reached.";
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