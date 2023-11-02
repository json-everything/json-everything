using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Operators;

internal class EvalOperator : IOperator
{
	public void Validate(JsonNode? template)
	{
		
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		throw new NotImplementedException();
	}
}