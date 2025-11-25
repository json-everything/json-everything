using System.Text.Json;

#pragma warning disable CS8618

namespace Json.Benchmarks.SchemaSuite;

public class TestCase
{
	public string Description { get; set; }
	public JsonElement Data { get; set; }
	public bool Valid { get; set; }
}