using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal interface IBinaryValueOperator
{
	JsonNode? Evaluate(JsonNode? left, JsonNode? right);
}