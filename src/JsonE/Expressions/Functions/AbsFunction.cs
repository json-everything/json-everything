using System;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions.Functions;

internal class AbsFunction : FunctionDefinition
{
	private const string _name = "abs";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var num = (arguments[0] as JsonValue)?.GetNumber();
		if (!num.HasValue)
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));

		return Math.Abs(num.Value);
	}
}
