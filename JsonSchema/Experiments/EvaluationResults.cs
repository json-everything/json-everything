using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.Experiments;

public class EvaluationResults
{
	[JsonPropertyName("valid")]
	public bool Valid { get; set; }
	[JsonPropertyName("schemaLocation")]
	public Uri SchemaLocation { get; set; }
	[JsonPropertyName("instanceLocation")]
	public JsonPointer InstanceLocation { get; set; }
	[JsonPropertyName("evaluationPath")]
	public JsonPointer EvaluationPath { get; set; }
	[JsonPropertyName("annotations")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyDictionary<string, JsonNode?>? Annotations { get; set; }
	[JsonPropertyName("details")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EvaluationResults[]? Details { get; set; }
}