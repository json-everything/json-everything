using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;

namespace Json.JsonE.Operators;

internal class SwitchOperator : IOperator
{
	public const string Name = "$switch";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name);

		var parameter = obj[Name];
		if (!parameter.IsTemplateOr<JsonObject>())
			throw new TemplateException("$switch can evaluate objects only");

		var valuesToCheck = parameter!.AsObject();

		// $default may be present but null-valued
		var def = valuesToCheck.FirstOrDefault(x => x.Key == "$default");

		var array = new JsonArray();
		foreach (var kvp in valuesToCheck.Where(x => x.Key != "$default").OrderBy(x => x.Key, StringComparer.Ordinal))
		{
			int index = 0;
			if (!ExpressionParser.TryParse(kvp.Key.AsSpan(), ref index, out var expr))
				throw new TemplateException("$switch keys must be valid expressions");

			var result = expr!.Evaluate(context);
			if (result is not JsonValue val || !val.TryGetValue(out bool b))
				throw new InterpreterException("$switch keys must evaluate to a boolean");

			if (b)
			{
				var value = JsonE.Evaluate(kvp.Value, context);
				array.Add(value.Clone());
			}
		}

		if (array.Count > 1)
			throw new TemplateException("$switch can only have one truthy condition");

		if (array.Count == 0)
		{
			if (def.Key == "$default") return JsonE.Evaluate(def.Value, context);

			return JsonE.DeleteMarker;
		}

		return array[0];
	}
}