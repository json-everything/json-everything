using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Functions;

internal class MaxFunction : FunctionDefinition
{
	public override string Name => "max";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.Number };
	public override bool AcceptsParamsList => true;
	public override FunctionValueType ReturnType => FunctionValueType.Number;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var nums = arguments.Select(x => (x as JsonValue)?.GetNumber()).ToArray();
		if (nums.Any(x => !x.HasValue))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		return nums.Max();
	}
}