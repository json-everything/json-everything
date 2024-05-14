using System.Text.Json.Nodes;

namespace Json.Patch;

internal class PatchContext
{
	public JsonNode? Source { get; set; }
	public string? Message { get; set; }
	public int Index { get; set; }

	public PatchContext(JsonNode? source)
	{
		Source = source;
	}
}