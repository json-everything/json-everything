using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

public class Requirement
{
	public int Priority { get; }
	public JsonPointer SubschemaPath { get; }
	public JsonPointer InstanceLocation { get; set; }

	public Func<JsonNode?, List<KeywordResult>, KeywordResult?> Evaluate { get; }

	public Requirement(JsonPointer subschemaPath, JsonPointer instanceLocation, Func<JsonNode?, List<KeywordResult>, KeywordResult?> evaluate, int priority = 0)
	{
		// TODO: schema location is schema's base uri + evaluation path after final $ref
		SubschemaPath = subschemaPath;
		InstanceLocation = instanceLocation;
		Evaluate = evaluate;
		Priority = priority;
	}
}