using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

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
			JsonArray a => EvaluateAsArray(a, eachTemplate),
			JsonObject o => EvaluateAsObject(o, eachTemplate),
			_ => throw new Exception("This shouldn't happen")
		};
	}

	private static JsonNode? EvaluateAsArray(JsonArray jsonArray, JsonNode? eachTemplate)
	{
		throw new NotImplementedException();
	}

	private static JsonNode? EvaluateAsObject(JsonObject jsonObject, JsonNode? eachTemplate)
	{
		throw new NotImplementedException();
	}
}