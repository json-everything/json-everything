using System;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Functions;

internal class AbsFunction : FunctionDefinition
{
	public override string Name => "abs";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.Number };
	public override FunctionValueType ReturnType => FunctionValueType.Number;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var num = (arguments[0] as JsonValue)?.GetNumber();
		if (!num.HasValue)
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		return Math.Abs(num.Value);
	}
}