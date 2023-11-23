using System.Text.Json.Nodes;

namespace Json.JsonE;

public delegate JsonNode? JsonFunctionDelegate(JsonNode?[] arguments, EvaluationContext context);

public class JsonFunction : FunctionDefinition
{
	public string Name { get; }

	private readonly JsonFunctionDelegate _function;

	private JsonFunction(JsonFunctionDelegate function)
	{
		_function = function;
	}

	public static JsonFunction Create(JsonFunctionDelegate function)
	{
		return new JsonFunction(function);
	}

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		return _function(arguments, context);
	}
}