using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Path;

internal class EvaluationContext
{
	public JsonNode? Root { get; }
	public List<PathMatch> Current { get; }
	public PathEvaluationOptions Options { get; }

	internal EvaluationContext(in JsonNode? root, PathEvaluationOptions options)
	{
		Root = root;
		Current = new List<PathMatch> { new(root, JsonPath.Root) };
		Options = options;
	}

	internal PathResult BuildResult()
	{
		return new PathResult(Current);
	}
}