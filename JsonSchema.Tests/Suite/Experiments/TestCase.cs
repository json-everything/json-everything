using System.Text.Json.Nodes;

#pragma warning disable CS8618

namespace Json.Schema.Tests.Suite.Experiments;

public class TestCase
{
	public string Description { get; set; }
	public JsonNode? Data { get; set; }
	public bool Valid { get; set; }
}