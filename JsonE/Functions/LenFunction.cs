using System.Globalization;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;

namespace Json.JsonE.Functions;

internal class LenFunction : FunctionDefinition
{
	public override string Name => "len";
	public override int[] ParameterCounts { get; } = { 1 };

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var arg = arguments[0];

		if (arg is JsonArray arr) return arr.Count;
		if (arg is JsonValue val && val.TryGetValue(out string? str)) return new StringInfo(str).LengthInTextElements;

		throw new BuiltInException(CommonErrors.IncorrectArgType(Name));
	}
}