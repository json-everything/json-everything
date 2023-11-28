using System;
using System.Globalization;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;

namespace Json.JsonE.Expressions.Functions;

internal class FromNowFunction : FunctionDefinition
{
	private const string _name = "fromNow";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));

		string? argNowStr = null;
		if (arguments.Length == 2 && (arguments[1] is not JsonValue argNowNode ||
									  !argNowNode.TryGetValue(out argNowStr)))
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));

		DateTime now;
		var interval = Interval.Parse(str);
		if (argNowStr != null)
		{
			if (!DateTime.TryParse(argNowStr, out now))
				throw new BuiltInException(CommonErrors.IncorrectArgType(_name));
		}
		else
		{
			var nowNode = context.Find(ContextAccessor.Now);
			if (nowNode is not JsonValue nowVal ||
				!nowVal.TryGetValue(out str) ||
				!DateTime.TryParse(str, out now))
				throw new BuiltInException(CommonErrors.IncorrectArgType(_name));
		}

		return interval.AddTo(now.ToUniversalTime()).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK", CultureInfo.InvariantCulture);
	}
}