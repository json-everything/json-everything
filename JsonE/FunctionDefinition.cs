using System.Text.Json.Nodes;

namespace Json.JsonE;

/// <summary>
/// Serves as a base type for JSON-e functions.  Use <see cref="JsonFunction"/> to create custom functions.
/// </summary>
public abstract class FunctionDefinition
{
	internal abstract JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context);

	/// <summary>
	/// Implicitly converts a <see cref="FunctionDefinition"/> into a <see cref="JsonNode"/>.
	/// </summary>
	/// <param name="func">The function.</param>
	public static implicit operator JsonNode?(FunctionDefinition func)
	{
		return JsonValue.Create(func);
	}
}