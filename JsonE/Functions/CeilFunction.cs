using System;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Functions;

internal class CeilFunction : FunctionDefinition
{
	public override string Name => "ceil";
	public override int[] ParameterCounts { get; } = { 1 };

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var num = (arguments[0] as JsonValue)?.GetNumber();
		if (!num.HasValue)
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		return Math.Ceiling(num.Value);
	}
}