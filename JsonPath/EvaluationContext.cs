using System.Collections.Generic;
using System.Text.Json;

namespace Json.Path;

internal class EvaluationContext
{
	public JsonElement Root { get; }
	public List<PathMatch> Current { get; }
	public PathEvaluationOptions Options { get; }

	internal EvaluationContext(in JsonElement root, PathEvaluationOptions options)
	{
		Root = root.Clone();
		Current = new List<PathMatch> { new(root, JsonPath.Root) };
		Options = options;
	}

	internal PathResult BuildResult()
	{
		return new PathResult(Current);
	}
}