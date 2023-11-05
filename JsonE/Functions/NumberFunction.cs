using System.Text.Json.Nodes;
using Json.JsonE.Operators;

namespace Json.JsonE.Functions;

internal class NumberFunction : FunctionDefinition
{
	public override string Name => "number";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.String };
	public override FunctionValueType ReturnType => FunctionValueType.Number;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		return decimal.TryParse(str, out var d)
			? d
			: throw new TemplateException("not a number");
	}
}