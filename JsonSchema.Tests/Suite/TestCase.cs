using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

#pragma warning disable CS8618

namespace Json.Schema.Tests.Suite;

public class TestCase
{
	public string Description { get; set; }
	public JsonNode? Data { get; set; }
	public bool Valid { get; set; }
	public Dictionary<string, JsonSchema>? Output { get; set; }
	public JsonNode? OutputUnit { get; set; }
	public string Operation { get; set; }
	public Dictionary<string, JsonPointer> ExpectedLocations { get; set; }
}