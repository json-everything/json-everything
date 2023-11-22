using System;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;

namespace Json.JsonE.Expressions.Functions;

internal class DefinedFunction : FunctionDefinition
{
	private const string _name = "defined";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new InterpreterException(CommonErrors.IncorrectArgType(_name));

		var i = 0;
		if (ContextAccessor.TryParse(str.AsSpan(), ref i, out var accessor))
			return context.IsDefined(accessor!);

		throw new InterpreterException($"'{str}' is not a valid context accessor");

	}
}