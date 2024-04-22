using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Patch;

internal class AddOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new AddOperationHandler();

	private AddOperationHandler()
	{
	}

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (operation.Path.Segments.Length == 0)
		{
			context.Source = operation.Value;
			return;
		}

		if (!operation.Path.EvaluateAndGetParent(context.Source, out var target))
		{
			context.Message = $"Target path `{operation.Path}` could not be reached.";
			return;
		}

		var lastPathSegment = operation.Path[^1];
		if (target is JsonObject objTarget)
		{
			objTarget[lastPathSegment.GetSegmentValue()] = operation.Value?.DeepClone();
			return;
		}

		if (target is JsonArray arrTarget)
		{
			int index;
			if (lastPathSegment.Length == 1 && lastPathSegment[0] == '-')
				index = arrTarget.Count;
			else if (!lastPathSegment.TryGetInt(out index))
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