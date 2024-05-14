using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.JsonE.Expressions;
using Json.More;

namespace Json.JsonE.Operators;

internal class FindOperator : IOperator
{
	private static readonly Regex _eachForm = new(@"^each\(\s*(?<var1>[a-zA-Z_][a-zA-Z0-9_]*)\s*(,\s*(?<var2>[a-zA-Z_][a-zA-Z0-9_]*))?\s*\)");

	public const string Name = "$find";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name, _eachForm);
	
		var value = JsonE.Evaluate(obj[Name], context);

		var eachEntry = obj.FirstOrDefault(x => x.Key != Name);
		if (eachEntry.Value is not JsonValue eachValue)
			throw new TemplateException($"each can evaluate string expressions only");
		var eachTemplate = eachValue.GetString();
		if (eachTemplate is null)
			throw new TemplateException($"each can evaluate string expressions only");

		if (value is not JsonArray array)
			throw new TemplateException(CommonErrors.IncorrectValueType(Name, "an array"));

		return EvaluateAsArray(array, eachEntry.Key, eachTemplate, context);
	}

	private static JsonNode? EvaluateAsArray(JsonArray value, string each, string template, EvaluationContext context)
	{
		var match = _eachForm.Match(each);
		var itemVar = match.Groups["var1"].Value;
		var indexVar = match.Groups["var2"].Success ? match.Groups["var2"].Value : null;

		var itemContext = new JsonObject
		{
			[itemVar] = null
		};
		if (indexVar != null)
			itemContext[indexVar] = 0;
		context.Push(itemContext);

		var found = false;
		JsonNode? result = null;
		for (int i = 0; i < value.Count; i++)
		{
			itemContext[itemVar] = value[i].Clone();
			if (indexVar != null)
				itemContext[indexVar] = i;

			int index = 0;
			if (!ExpressionParser.TryParse(template.AsSpan(), ref index, out var expr))
				throw new TemplateException("$match keys must be valid expressions");

			var localResult = expr!.Evaluate(context);
			if (localResult.IsEquivalentTo(true))
			{
				found = true;
				result = itemContext[itemVar];
				break;
			}
		}

		context.Pop();
		return found ? result : JsonE.DeleteMarker;
	}
}