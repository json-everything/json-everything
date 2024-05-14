using System;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;

namespace Json.JsonE.Operators;

internal class EvalOperator : IOperator
{
	public const string Name = "$eval";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name);

		var parameter = obj[Name];
		if (parameter is not JsonValue value || !value.TryGetValue(out string? source))
			throw new TemplateException("$eval must be given a string expression");

		var expression = ExpressionParser.Parse(source.AsSpan());

		var jsonNode = expression.Evaluate(context);
		return jsonNode;
	}
}