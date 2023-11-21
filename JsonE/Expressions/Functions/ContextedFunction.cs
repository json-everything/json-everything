using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Functions;

internal class ContextedFunction : FunctionDefinition
{
	public override string Name { get; }

	public ContextedFunction(string name)
	{
		Name = name;
	}

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var index = 0;
		if (!ContextAccessor.TryParse(Name.AsSpan(), ref index, out var accessor))
			throw new InterpreterException($"Cannot find function `{Name}`");

		var actualFunctionName = (context.Find(accessor!) as JsonValue)?.GetValue<string>() ??
		                         throw new InterpreterException("function name must be a string");
		var actualFunction = FunctionRepository.Get(actualFunctionName);
		if (actualFunction is ContextedFunction)
			throw new InterpreterException($"Cannot find function `{actualFunctionName}`");

		return actualFunction!.Invoke(arguments, context);
	}
}