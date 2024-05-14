using System;
using System.Text.Json.Nodes;

namespace Json.Patch;

internal class MoveOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new MoveOperationHandler();

	private MoveOperationHandler() { }

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (Equals(operation.Path, operation.From)) return;

		if (operation.Path.Count == 0)
		{
			context.Message = "Cannot move root value.";
			return;
		}

		if (!operation.From.EvaluateAndGetParent(context.Source, out var source) ||
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

		var lastFromSegment = operation.From[^1];
		if (source is JsonObject objSource)
			objSource.Remove(lastFromSegment);
		else if (source is JsonArray arrSource)
		{
			var index = lastFromSegment.Length == 0 && lastFromSegment[0] == '-'
				? arrSource.Count
				: int.TryParse(lastFromSegment, out var i)
					? i
					: throw new ArgumentException("Expected integer");
			arrSource.RemoveAt(index);
		}

		if (operation.Path.Count == 0)
		{
			context.Source = data;
			return;
		}

		var lastPathSegment = operation.Path[^1];
		if (target is JsonObject objTarget)
		{
			objTarget[lastPathSegment] = data?.DeepClone();
			return;
		}

		if (target is JsonArray arrTarget)
		{
			int index;
			if (lastPathSegment.Length == 0 && lastPathSegment[0] == '-')
				index = arrTarget.Count;
			else if (!int.TryParse(lastPathSegment, out index))
			{
				context.Message = $"Target path `{operation.Path}` could not be reached.";
				return;
			}
			if (0 <= index && index < arrTarget.Count)
				arrTarget.Insert(index, data?.DeepClone());
			else if (index == arrTarget.Count)
				arrTarget.Add(data?.DeepClone());
			else
				context.Message = "Path indicates an index greater than the bounds of the array";
		}
	}
}