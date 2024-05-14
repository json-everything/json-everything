using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Operators;

internal class ReverseOperator : IOperator
{ 
	public const string Name = "$reverse";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name);

		var value = obj[Name]!;
		var array = JsonE.Evaluate(value, context) as JsonArray ??
		            throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array of objects"));

		return array.Reverse().ToJsonArray();
	}
}