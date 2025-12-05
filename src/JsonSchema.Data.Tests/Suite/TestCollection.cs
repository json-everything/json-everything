using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
#pragma warning disable CS8618

namespace Json.Schema.Data.Tests.Suite;

public class TestCollection
{
	[JsonPropertyName("description")]
	public string Description { get; set; }
	[JsonPropertyName("schema")]
	public JsonElement Schema { get; set; }
	[JsonPropertyName("tests")]
	public List<TestCase> Tests { get; set; }
	[JsonPropertyName("isOptional")]
	public bool IsOptional { get; set; }
}
