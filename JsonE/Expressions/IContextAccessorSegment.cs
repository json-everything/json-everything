using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal interface IContextAccessorSegment
{
	bool TryFind(JsonNode? target, out JsonNode? value);
}