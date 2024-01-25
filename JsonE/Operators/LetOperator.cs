using System.Text.Json.Nodes;

namespace Json.JsonE.Operators;

internal class LetOperator : IOperator
{
	public const string Name = "$let";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name, "in");

		if (obj.Count != 2)
			throw new TemplateException("$let operator requires an `in` clause");
	
		var value = obj[Name];

		var additionalContext = JsonE.Evaluate(value, context);
		additionalContext.ValidateAsContext(Name);

		context.Push((JsonObject)additionalContext!);
		var result = JsonE.Evaluate(obj["in"], context);
		context.Pop();

		return result;
	}
}