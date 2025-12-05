using System.Collections.Generic;
using System.Text.Json;

namespace Json.Schema.Data.Keywords;

internal class DataSpec
{
	public Dictionary<string, IDataResourceIdentifier> References { get; } = new();
	public Dictionary<string, JsonElement> Data { get; } = new();
	public List<string> Unresolved { get; } = new();
}