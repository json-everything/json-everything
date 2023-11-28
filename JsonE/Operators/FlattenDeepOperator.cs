using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Operators;

internal class FlattenDeepOperator : IOperator
{ 
	public const string Name = "$flattenDeep";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name);

		var value = obj[Name];
		var array = JsonE.Evaluate(value, context) as JsonArray ??
		            throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array"));

		return array.SelectMany(Flatten).ToJsonArray();
	}

	private static IEnumerable<JsonNode?> Flatten(JsonNode? node)
	{
		if (node is not JsonArray arr)
		{
			yield return node;
			yield break;
		}

		foreach (var item in arr)
		{
			foreach (var child in Flatten(item))
			{
				yield return child;
			}
		}
	}
}