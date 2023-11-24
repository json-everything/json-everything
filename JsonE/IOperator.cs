using System.Text.Json.Nodes;

namespace Json.JsonE;

internal interface IOperator
{
	JsonNode? Evaluate(JsonNode? template, EvaluationContext context);
}