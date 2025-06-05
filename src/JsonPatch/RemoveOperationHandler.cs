﻿using System;
using System.Text.Json.Nodes;

namespace Json.Patch;

internal class RemoveOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new RemoveOperationHandler();

	private RemoveOperationHandler() { }

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (operation.Path.SegmentCount == 0)
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

		var lastPathSegment = operation.Path.GetSegment(operation.Path.SegmentCount - 1).ToString();
		if (source is JsonObject objSource)
			objSource.Remove(lastPathSegment);
		else if (source is JsonArray arrSource)
		{
			var index = lastPathSegment.Length != 0 && lastPathSegment[0] == '-'
				? arrSource.Count - 1
				: int.TryParse(lastPathSegment, out var i)
					? i
					: throw new ArgumentException("Expected integer");
			arrSource.RemoveAt(index);
		}
	}
}