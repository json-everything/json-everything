using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Patch;

internal class RemoveOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new RemoveOperationHandler();

	private RemoveOperationHandler() { }

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (operation.Path.OldSegments.Length == 0)
		{
			context.Message = "Cannot remove root value.";
			return;
		}

		if (!operation.Path.EvaluateAndGetParent(context.Source, out var source) ||
			!operation.Path.TryEvaluate(context.Source, out _))
		{
			context.Message = $"Target path `{operation.Path}` could not be reached.";
			return;
		}

		var lastPathSegment = operation.Path.OldSegments.Last().Value;
		if (source is JsonObject objSource)
			objSource.Remove(lastPathSegment);
		else if (source is JsonArray arrSource)
		{
			var index = lastPathSegment == "-" ? arrSource.Count - 1 : int.Parse(lastPathSegment);
			arrSource.RemoveAt(index);
		}
	}
}