using System.Text.Json.Nodes;

namespace Json.JsonE;

/// <summary>
/// Defines a signature for custom JSON-e functions.
/// </summary>
/// <param name="arguments"></param>
/// <param name="context"></param>
/// <returns></returns>
public delegate JsonNode? JsonFunctionDelegate(JsonNode?[] arguments, EvaluationContext context);

/// <summary>
/// Serves as a <see cref="JsonNode"/>-compatible wrapper for a custom JSON-e function.
/// </summary>
public class JsonFunction : FunctionDefinition
{
	private readonly JsonFunctionDelegate _function;

	private JsonFunction(JsonFunctionDelegate function)
	{
		_function = function;
	}

	/// <summary>
	/// Creates a new <see cref="JsonFunction"/>.
	/// </summary>
	/// <param name="function"></param>
	/// <returns></returns>
	public static JsonFunction Create(JsonFunctionDelegate function)
	{
		return new JsonFunction(function);
	}

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		return _function(arguments, context);
	}
}