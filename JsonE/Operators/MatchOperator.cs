using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.More;

namespace Json.JsonE.Operators;

internal class MatchOperator : IOperator
{
	public const string Name = "$match";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name);
	
		var value = JsonE.Evaluate(obj[Name], context) as JsonObject ??
			throw new TemplateException("$match can evaluate objects only");

		var array = new JsonArray();
		foreach (var kvp in value.OrderBy(x => x.Key, StringComparer.Ordinal))
		{
			int index = 0;
			if (!ExpressionParser.TryParse(kvp.Key.AsSpan(), ref index, out var expr))
				throw new TemplateException("$match keys must be valid expressions");

			var result = expr!.Evaluate(context);
			if (result is not JsonValue val || !val.TryGetValue(out bool b))
				throw new InterpreterException("$match keys must evaluate to a boolean");

			if (b) array.Add(kvp.Value.Clone());
		}

		return array;
	}
}