using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Functions;

public class JsonFunction : FunctionDefinition
{
	public string Name { get; }
	
	private readonly Func<JsonNode?[], EvaluationContext, JsonNode> _function;

	private JsonFunction(Func<JsonNode?[], EvaluationContext, JsonNode> function)
	{
		_function = function;
	}

	public static JsonFunction Create(Func<JsonNode?[], EvaluationContext, JsonNode> function)
	{
		return new JsonFunction(function);
	}

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		return _function(arguments, context);
	}
}