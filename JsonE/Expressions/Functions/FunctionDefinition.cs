using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Functions;

internal abstract class FunctionDefinition
{
	public abstract string Name { get; }

	internal abstract JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context);

	public static implicit operator JsonNode?(FunctionDefinition func)
	{
		return JsonValue.Create(func);
	}
}
