using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace Json.Schema.Data.Tests.Suite;

public class TestCase
{
	[JsonPropertyName("description")]
	public string Description { get; set; }
	[JsonPropertyName("data")]
	public JsonNode? Data { get; set; }
	[JsonPropertyName("valid")]
	public bool Valid { get; set; }
	[JsonPropertyName("error")]
	public bool Error { get; set; }
}