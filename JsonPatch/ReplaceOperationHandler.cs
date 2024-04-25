using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Patch;

internal class ReplaceOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new ReplaceOperationHandler();

	private ReplaceOperationHandler() { }

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (operation.Path.SegmentCount == 0)
		{
			context.Source = operation.Value;
			return;
		}

		if (!operation.Path.EvaluateAndGetParent(context.Source, out var target) ||
			!operation.Path.TryEvaluate(context.Source, out _))
		{
			context.Message = $"Target path `{operation.Path}` could not be reached.";
			return;
		}

		var lastPathSegment = operation.Path[^1];
		if (target is JsonObject objTarget)
		{
			objTarget[lastPathSegment.GetSegmentName()] = operation.Value?.DeepClone();
			return;
		}

		if (target is JsonArray arrTarget)
		{
			int index;
			if (lastPathSegment.Length == 0 && lastPathSegment[0] == '-')
				index = arrTarget.Count;
			else if (!lastPathSegment.TryGetInt(out index))
			{
				context.Message = $"Target path `{operation.Path}` could not be reached.";
				return;
			}
			if (0 <= index && index < arrTarget.Count)
				arrTarget[index] = operation.Value?.DeepClone();
			else if (index == arrTarget.Count)
				arrTarget.Add(operation.Value?.DeepClone());
			else
				context.Message = "Path indicates an index greater than the bounds of the array";
		}
	}
}