using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.JsonE.Operators;

internal class MapOperator : IOperator
{
	private static readonly Regex _byForm = new(@"^each\(\s*(?<var1>[a-zA-Z_][a-zA-Z0-9_]*)\s*(,\s*(?<var2>[a-zA-Z_][a-zA-Z0-9_]*))?\s*\)");

	public const string Name = "$map";
	
	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name, _byForm);

		var parameter = obj[Name];
		if (parameter.IsTemplateOr<JsonObject>() || parameter.IsTemplateOr<JsonArray>()) return;

		throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array or object"));
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		var value = JsonE.Evaluate(obj[Name], context);

		var eachEntry = obj.FirstOrDefault(x => x.Key != Name);
		var eachTemplate = eachEntry.Value;

		return value switch
		{
			JsonArray a => EvaluateAsArray(a, eachEntry.Key, eachTemplate, context),
			JsonObject o => EvaluateAsObject(o, eachEntry.Key, eachTemplate, context),
			_ => throw new Exception("This shouldn't happen")
		};
	}

	private static JsonNode? EvaluateAsArray(JsonArray value, string each, JsonNode? template, EvaluationContext context)
	{
		var match = _byForm.Match(each);
		var itemVar = match.Groups["var1"].Value;
		var indexVar = match.Groups["var2"].Success ? match.Groups["var2"].Value : null;

		var itemContext = new JsonObject
		{
			[itemVar] = null
		};
		if (indexVar != null)
			itemContext[indexVar] = 0;
		context.Push(itemContext);

		var array = new JsonArray();
		for (int i = 0; i < value.Count; i++)
		{
			itemContext[itemVar] = value[i].Copy();
			if (indexVar != null)
				itemContext[indexVar] = i;

			array.Add(JsonE.Evaluate(template, context).Copy());
		}

		context.Pop();
		return array;
	}

	private static JsonNode? EvaluateAsObject(JsonObject value, string each, JsonNode? template, EvaluationContext context)
	{
		var match = _byForm.Match(each);
		var valueVar = match.Groups["var1"].Value;
		var keyVar = match.Groups["var2"].Success ? match.Groups["var2"].Value : null;

		var itemContext = new JsonObject();
		if (keyVar != null)
		{
			itemContext[valueVar] = null;
			itemContext[keyVar] = 0;
		}
		else
		{
			itemContext[valueVar] = new JsonObject
			{
				["key"] = null,
				["val"] = null
			};
		}
		context.Push(itemContext);

		var obj = new JsonObject();
		foreach (var kvp in value)
		{
			if (keyVar != null)
			{
				itemContext[valueVar] = kvp.Value.Copy();
				itemContext[keyVar] = kvp.Key;
			}
			else
			{
				itemContext[valueVar]!["val"] = kvp.Value.Copy();
				itemContext[valueVar]!["key"] = kvp.Key;
			}

			MergeOperator.Merge(obj, JsonE.Evaluate(template, context));
		}

		context.Pop();
		return obj;
	}
}