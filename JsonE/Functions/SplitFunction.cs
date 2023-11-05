using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Functions;

internal class SplitFunction : FunctionDefinition
{
	public override string Name => "split";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.String, FunctionValueType.String };
	public override FunctionValueType ReturnType => FunctionValueType.Array;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		throw new NotImplementedException();
	}
}