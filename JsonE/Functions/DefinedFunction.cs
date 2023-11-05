using System;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.JsonE.Operators;

namespace Json.JsonE.Functions;

internal class DefinedFunction : FunctionDefinition
{
	public override string Name => "defined";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.String };
	public override FunctionValueType ReturnType => FunctionValueType.Boolean;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		var i = 0;
		if (ContextAccessor.TryParse(str.AsSpan(), ref i, out var accessor))
			return context.IsDefined(accessor!);

		throw new InterpreterException($"'{str}' is not a valid context accessor");

	}
}