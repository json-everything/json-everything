using System.Linq;
using System.Text.Json.Nodes;

namespace Json.JsonE.Operators;

internal class MergeOperator : IOperator
{ 
	public const string Name = "$merge";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name);

		var value = obj[Name]!;
		var array = JsonE.Evaluate(value, context) as JsonArray ??
		            throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array of objects"));

		return array.Aggregate(new JsonObject(), Merge);
	}

	public static JsonObject Merge(JsonObject result, JsonNode? current)
	{
		var obj = current as JsonObject ?? throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array of objects"));

		foreach (var kvp in obj)
		{
			result[kvp.Key] = kvp.Value.Clone();
		}

		return result;
	}
}