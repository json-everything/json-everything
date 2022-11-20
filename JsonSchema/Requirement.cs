using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

public class Requirement
{
	public delegate KeywordResult? EvaluationFunction(JsonNode? node, List<KeywordResult> resultCache, Dictionary<JsonPointer, JsonNode?> instanceCatalog);

	public int Priority { get; }
	public JsonPointer SubschemaPath { get; }
	public JsonPointer InstanceLocation { get; set; }

	public EvaluationFunction Evaluate { get; }

	public Requirement(JsonPointer subschemaPath, JsonPointer instanceLocation, EvaluationFunction evaluate, int priority = 0)
	{
		SubschemaPath = subschemaPath;
		InstanceLocation = instanceLocation;
		Evaluate = evaluate;
		Priority = priority;
	}
}