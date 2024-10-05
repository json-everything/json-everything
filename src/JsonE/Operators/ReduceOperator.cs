using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Json.JsonE.Operators;

internal class ReduceOperator : IOperator
{ 
	private static readonly Regex _byForm = new(@"^each\(\s*(?<var1>[a-zA-Z_][a-zA-Z0-9_]*)\s*(,\s*(?<var2>[a-zA-Z_][a-zA-Z0-9_]*))?(,\s*(?<var3>[a-zA-Z_][a-zA-Z0-9_]*))?\s*\)");

	public const string Name = "$reduce";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name, "initial", _byForm);

		var value = JsonE.Evaluate(obj[Name], context);
		if (value is not JsonArray items)
			throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array"));

		var initial = JsonE.Evaluate(obj["initial"], context);

		var eachEntry = obj.FirstOrDefault(x => x.Key is not (Name or "initial"));
		var eachTemplate = eachEntry.Value;

		return EvaluateAsArray(items, initial, eachEntry.Key, eachTemplate, context);
	}

	private static JsonNode? EvaluateAsArray(JsonArray value, JsonNode? initial, string each, JsonNode? template, EvaluationContext context)
	{
		var match = _byForm.Match(each);
		var accVar = match.Groups["var1"].Value;
		var itemVar = match.Groups["var2"].Value;
		var indexVar = match.Groups["var3"].Success ? match.Groups["var3"].Value : null;

		var itemContext = new JsonObject
		{
			[accVar] = initial?.Clone(),
			[itemVar] = null
		};
		if (indexVar != null)
			itemContext[indexVar] = 0;

		context.Push(itemContext);

		for (var i = 0; i < value.Count; i++)
		{
			itemContext[itemVar] = value[i].Clone();
			if (indexVar != null)
				itemContext[indexVar] = i;

			var localResult = JsonE.Evaluate(template, context);
			if (!ReferenceEquals(localResult, JsonE.DeleteMarker))
				itemContext[accVar] = localResult.Clone();
		}

		context.Pop();
		return itemContext[accVar];
	}
}