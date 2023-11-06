using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;

using static Json.JsonE.Operators.CommonErrors;

namespace Json.JsonE.Operators;

internal class EvalOperator : IOperator
{
	public const string Name = "$eval";

	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name);

		var parameter = obj[Name];
		if (parameter is not JsonValue value || !value.TryGetValue(out string? source))
			throw new TemplateException("$eval must be given a string expression");

		int index = 0;
		if (!ExpressionParser.TryParse(source.AsSpan(), ref index, out _))
			throw new TemplateException("Expression is not valid");
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();

		if (obj.Count > 1)
			throw new TemplateException(UndefinedProperties(Name, obj.Select(x => x.Key).Where(x => x != Name)));

		var source = obj["$eval"]!.GetValue<string>();

		int index = 0;
		if (!ExpressionParser.TryParse(source.AsSpan(), ref index, out var expression))
			throw new TemplateException("Expression is not valid");

		return expression!.Evaluate(context);
	}
}