using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Operators;

internal class MergeOperator : IOperator
{ 
	public const string Name = "$merge";

	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name);

		var parameter = obj[Name];
		if (parameter is JsonArray arr && arr.All(x => x.IsTemplateOr<JsonObject>()) ||
		    parameter.TryGetTemplate(out _))
			return;

		throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array of objects"));
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var value = template!.AsObject()[Name]!;
		var array = value.TryGetTemplate(out var t)
			? t!.Evaluate(context) as JsonArray ?? throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array of objects"))
			: value.AsArray();

		return array.Aggregate(new JsonObject(), Merge);
	}

	private static JsonObject Merge(JsonObject result, JsonNode? current)
	{
		var obj = current!.AsObject();

		foreach (var kvp in obj)
		{
			result[kvp.Key] = kvp.Value.Copy();
		}

		return result;
	}
}