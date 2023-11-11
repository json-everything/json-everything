using System.Text.Json.Nodes;
using Json.JsonE.Operators;

namespace Json.JsonE.Functions;

internal class StripFunction : FunctionDefinition
{
	public override string Name => "strip";
	public override int[] ParameterCounts { get; } = { 1 };

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		return str.Trim();
	}
}