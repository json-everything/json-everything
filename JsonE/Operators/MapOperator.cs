using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.JsonE.Operators;

internal class MapOperator : IOperator
{
	private static readonly Regex _byForm = new(@"^each\(\s*(?<var1>[a-zA-Z_][a-zA-Z0-9_]*)\s*(,\s*(?<var2>[a-zA-Z_][a-zA-Z0-9_]*))?\s*\)");

	public const string Name = "$map";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name, _byForm);
	
		var value = JsonE.Evaluate(obj[Name], context);

		var eachEntry = obj.FirstOrDefault(x => x.Key != Name);
		var eachTemplate = eachEntry.Value;

		return value switch
		{
			JsonArray a => EvaluateAsArray(a, eachEntry.Key, eachTemplate, context),
			JsonObject o => EvaluateAsObject(o, eachEntry.Key, eachTemplate, context),
			_ => throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array or object"))
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
			itemContext[itemVar] = value[i].Clone();
			if (indexVar != null)
				itemContext[indexVar] = i;

			var localResult = JsonE.Evaluate(template, context);
			if (!ReferenceEquals(localResult, JsonE.DeleteMarker))
				array.Add(localResult.Clone());
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
				itemContext[valueVar] = kvp.Value.Clone();
				itemContext[keyVar] = kvp.Key;
			}
			else
			{
				itemContext[valueVar]!["val"] = kvp.Value.Clone();
				itemContext[valueVar]!["key"] = kvp.Key;
			}

			var evaluated = JsonE.Evaluate(template, context) as JsonObject ?? throw new TemplateException("$map on objects expects each(x) to evaluate to an object");
			MergeOperator.Merge(obj, evaluated);
		}

		context.Pop();
		return obj;
	}
}