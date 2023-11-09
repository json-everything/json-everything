using System;
using System.Globalization;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.JsonE.Functions;

namespace Json.JsonE.Operators;

internal class FromNowOperator : IOperator
{
	public const string Name = "$fromNow";
	
	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name, "from");

		var parameter = obj[Name];
		if (parameter.IsTemplateOr<string>()) return;

		if (obj.Count > 1)
		{
			parameter = obj["from"];
			if (parameter.IsTemplateOr<string>()) return;
		}
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		var value = obj[Name];
		var intervalStr = value.TryGetTemplate(out var t)
			? t!.Evaluate(context) as JsonValue ?? throw new TemplateException(CommonErrors.IncorrectValueType(Name, "a string"))
			: value!.AsValue();

		if (!intervalStr.TryGetValue(out string? str))
			throw new TemplateException("$fromNow expects a string");

		string? argFromStr = null;
		if (obj.Count == 2)
		{
			value = obj["from"];
			var fromStr = value.TryGetTemplate(out t)
				? t!.Evaluate(context) as JsonValue ?? throw new TemplateException(CommonErrors.IncorrectValueType(Name, "a string"))
				: value!.AsValue();

			if (!fromStr.TryGetValue(out argFromStr))
				throw new TemplateException("$fromNow expects a string");
		}

		DateTime now;
		var interval = Interval.Parse(str);
		if (argFromStr != null)
		{
			if (!DateTime.TryParse(argFromStr, out now))
				throw new InterpreterException(CommonErrors.IncorrectArgType(Name));
		}
		else
		{
			var nowNode = context.Find(ContextAccessor.Now);
			if (nowNode is not JsonValue nowVal ||
			    !nowVal.TryGetValue(out str) ||
			    !DateTime.TryParse(str, out now))
				throw new InterpreterException(CommonErrors.IncorrectArgType(Name));
		}

		return interval.AddTo(now.ToUniversalTime()).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK", CultureInfo.InvariantCulture);
	}
}