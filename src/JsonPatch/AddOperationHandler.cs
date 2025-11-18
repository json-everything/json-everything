using System.Text.Json.Nodes;

namespace Json.Patch;

internal class AddOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new AddOperationHandler();

	private AddOperationHandler()
	{
	}

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (operation.Path.SegmentCount == 0)
		{
			context.Source = operation.Value;
			return;
		}

		if (!operation.Path.EvaluateAndGetParent(context.Source, out var target))
		{
			context.Message = $"Target path `{operation.Path}` could not be reached.";
			return;
		}

		var lastPathSegment = operation.Path.GetLocal().GetSegment(0).ToString();
		if (target is JsonObject objTarget)
		{
			objTarget[lastPathSegment] = operation.Value?.DeepClone();
			return;
		}

		if (target is JsonArray arrTarget)
		{
			int index;
			if (lastPathSegment is ['-'])
				index = arrTarget.Count;
			else if (!int.TryParse(lastPathSegment, out index))
			{
				context.Message = $"Target path `{operation.Path}` could not be reached.";
				return;
			}
			if (0 <= index && index < arrTarget.Count)
				arrTarget.Insert(index, operation.Value?.DeepClone());
			else if (index == arrTarget.Count)
				arrTarget.Add(operation.Value?.DeepClone());
			else
				context.Message = "Path indicates an index greater than the bounds of the array";
		}
	}
}