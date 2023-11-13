using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Operators;

internal class SwitchOperator : IOperator
{
	public const string Name = "$switch";
	
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