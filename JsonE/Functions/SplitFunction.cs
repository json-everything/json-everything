using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Functions;

internal class SplitFunction : FunctionDefinition
{
	public override string Name => "split";
	public override int[] ParameterCounts { get; } = { 1 };

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		throw new NotImplementedException();
	}
}