using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.More;

namespace Json.JsonE.Operators;

internal class IfThenElseOperator : IOperator
{
	public const string Name = "$if";

	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name, "then", "else");

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
		var source = obj[Name]!.GetValue<string>();

		int index = 0;
		if (!ExpressionParser.TryParse(source.AsSpan(), ref index, out var expression))
			throw new TemplateException("Expression is not valid");

		var cond = expression!.Evaluate(context);
		obj.TryGetValue("then", out var thenValue, out _);
		obj.TryGetValue("else", out var elseValue, out _);

		if (thenValue.TryGetTemplate(out var thenTemplate))
			thenValue = thenTemplate!.Evaluate(context);
		if (elseValue.TryGetTemplate(out var elseTemplate))
			elseValue = elseTemplate!.Evaluate(context);

		return cond.IsTruthy() ? thenValue : elseValue;
	}
}