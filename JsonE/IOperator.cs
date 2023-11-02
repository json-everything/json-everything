using System.Text.Json.Nodes;

namespace Json.JsonE;

internal interface IOperator
{
	void Validate(JsonNode? template);
	JsonNode? Evaluate(JsonNode? template, EvaluationContext context);
}