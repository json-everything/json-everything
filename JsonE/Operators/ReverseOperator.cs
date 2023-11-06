using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Operators;

internal class ReverseOperator : IOperator
{ 
	public const string Name = "$reverse";

	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name);

		var parameter = obj[Name];
		if (parameter.IsTemplateOr<JsonArray>())  return;

		throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array of objects"));
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var value = template!.AsObject()[Name]!;
		var array = value.TryGetTemplate(out var t)
			? t!.Evaluate(context) as JsonArray ?? throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array of objects"))
			: value.AsArray();

		return array.Reverse().ToJsonArray();
	}
}