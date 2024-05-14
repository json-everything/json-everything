using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal interface IContextAccessorSegment
{
	bool TryFind(JsonNode? contextValue, EvaluationContext fullContext, out JsonNode? value);
}