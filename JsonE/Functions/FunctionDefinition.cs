using System.Text.Json.Nodes;

namespace Json.JsonE.Functions;

internal abstract class FunctionDefinition
{
	public abstract string Name { get; }

	public abstract int[] ParameterCounts { get; }

	public virtual bool AcceptsParamsList => false;

	internal abstract JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context);

	public static implicit operator JsonNode?(FunctionDefinition func)
	{
		return JsonValue.Create(func);
	}
}
