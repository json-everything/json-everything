using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal interface IContextAccessorSegment
{
	bool TryFind(JsonNode? contextValue, out JsonNode? value);
}