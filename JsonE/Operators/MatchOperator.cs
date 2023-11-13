using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Operators;

internal class MatchOperator : IOperator
{
	public const string Name = "$match";
	
	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name);
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		throw new NotImplementedException();
	}
}