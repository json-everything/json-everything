using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using static Json.JsonE.Operators.CommonErrors;

namespace Json.JsonE.Operators;

internal class FlattenOperator : IOperator
{ 
	public const string Name = "$flatten";

	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name);

		var parameter = obj[Name];
		if (parameter.IsTemplateOr<JsonArray>()) return;

		throw new TemplateException(IncorrectValueType(Name, "an array"));
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var value = template!.AsObject()[Name]!;
		var array = value.TryGetTemplate(out var t)
			? t!.Evaluate(context) as JsonArray ?? throw new TemplateException(IncorrectValueType(Name, "an array"))
			: value.AsArray();

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
			yield return item;
		}
	}
}