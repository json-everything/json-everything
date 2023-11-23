using System.Text.Json.Nodes;

namespace Json.JsonE;

public abstract class FunctionDefinition
{
	internal abstract JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context);

	public static implicit operator JsonNode?(FunctionDefinition func)
	{
		return JsonValue.Create(func);
	}
}