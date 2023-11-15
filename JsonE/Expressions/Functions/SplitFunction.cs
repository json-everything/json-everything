using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Functions;

internal class SplitFunction : FunctionDefinition
{
	public override string Name => "split";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		throw new NotImplementedException();
	}
}