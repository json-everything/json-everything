using System.Collections.Generic;
using System.Text.Json;
#pragma warning disable CS8618

namespace Json.Schema.Tests.Suite;

public class TestCase
{
	public string Description { get; set; }
	public JsonElement Data { get; set; }
	public bool Valid { get; set; }
	//public Dictionary<string, JsonSchema>? Output { get; set; }
}