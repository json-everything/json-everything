using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Patch;

internal class CopyOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new CopyOperationHandler();

	private CopyOperationHandler() { }

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (Equals(operation.Path, operation.From)) return;

		if (!operation.From.EvaluateAndGetParent(context.Source, out _) ||
			!operation.From.TryEvaluate(context.Source, out var data))
		{
			context.Message = $"Source path `{operation.Path}` could not be reached.";
			return;
		}

		if (!operation.Path.EvaluateAndGetParent(context.Source, out var target))
		{
			context.Message = $"Target path `{operation.From}` could not be reached.";
			return;
		}

		if (operation.Path.OldSegments.Length == 0)
		{
			context.Source = data;
			return;
		}

		var lastPathSegment = operation.Path.OldSegments.Last().Value;
		if (target is JsonObject objTarget)
		{
			objTarget[lastPathSegment] = data?.DeepClone();
			return;
		}

		if (target is JsonArray arrTarget)
		{
			var index = lastPathSegment == "-" ? arrTarget.Count : int.Parse(lastPathSegment);
			if (0 < index || index < arrTarget.Count)
				arrTarget[index] = data?.DeepClone();
			else if (index == arrTarget.Count)
				arrTarget.Add(data?.DeepClone());
		}
	}
}