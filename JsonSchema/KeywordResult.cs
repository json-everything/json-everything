using System;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

public class KeywordResult
{
	public JsonPointer SubschemaPath { get; set; }
	public string Keyword { get; set; }
	public Uri SchemaLocation { get; set; }
	public JsonPointer InstanceLocation { get; set; }

	// use JsonNull for null
	public JsonNode? Annotation { get; set; }
	public string? Error { get; set; }
	public bool? ValidationResult { get; set; }
}