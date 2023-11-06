using System;
using System.Globalization;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.JsonE.Operators;

namespace Json.JsonE.Functions;

internal class FromNowFunction : FunctionDefinition
{
	public override string Name => "fromNow";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.String, FunctionValueType.String };
	public override FunctionValueType ReturnType => FunctionValueType.String;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		string? argNowStr = null;
		if (arguments.Length == 2 && (arguments[1] is not JsonValue argNowNode ||
		                              !argNowNode.TryGetValue(out argNowStr)))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		DateTime now;
		var interval = Interval.ParseAndGetTimeSpan(str);
		if (argNowStr != null)
		{
			if (!DateTime.TryParse(argNowStr, out now))
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

		return now.ToUniversalTime().Add(interval).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK", CultureInfo.InvariantCulture);
	}
}