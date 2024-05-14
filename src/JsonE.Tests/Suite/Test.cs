using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.JsonE.Tests.Suite;

public class Test
{
#pragma warning disable CS8618
	[JsonPropertyName("title")]
	public string Title { get; set;}
	[JsonPropertyName("template")]
	public JsonNode? Template { get; set; }
	[JsonPropertyName("context")]
	public JsonNode? Context { get; set; }
	[JsonPropertyName("result")]
	public JsonNode? Expected { get; set; }
	[JsonPropertyName("error")]
	public JsonNode? ErrorNode { get; set; }

	public string? Error => ErrorNode is JsonValue value && value.TryGetValue<string>(out var s) ? s : null;
	public bool HasError => ErrorNode is JsonValue value &&
	                        (value.TryGetValue<string>(out _) ||
	                         (value.TryGetValue<bool>(out var b) && b));
#pragma warning restore CS8618
}
