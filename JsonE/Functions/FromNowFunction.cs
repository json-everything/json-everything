using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Functions;

internal class FromNowFunction : FunctionDefinition
{
	public override string Name => "fromNow";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.String };
	public override FunctionValueType ReturnType => FunctionValueType.String;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		throw new NotImplementedException();
	}
}