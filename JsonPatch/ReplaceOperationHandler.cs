using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Patch;

internal class ReplaceOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new ReplaceOperationHandler();

	private ReplaceOperationHandler() { }

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (operation.Path.Segments.Length == 0)
		{
			context.Source = operation.Value;
			return;
		}

		if (!operation.Path.EvaluateAndGetParent(context.Source, out var target))
		{
			context.Message = $"Target path `{operation.From}` could not be reached.";
			return;
		}

		var lastPathSegment = operation.Path.Segments.Last().Value;
		if (target is JsonObject objTarget)
		{
			objTarget[lastPathSegment] = operation.Value;
			return;
		}

		if (target is JsonArray arrTarget)
		{
			int index;
			if (lastPathSegment == "-")
				index = arrTarget.Count;
			else if (!int.TryParse(lastPathSegment, out index))
			{
				context.Message = $"Target path `{operation.Path}` could not be reached.";
				return;
			}
			if (0 <= index && index < arrTarget.Count)
				arrTarget[index] = operation.Value;
			else if (index == arrTarget.Count)
				arrTarget.Add(operation.Value);
			else
				context.Message = "Path indicates an index greater than the bounds of the array";
		}
	}
}