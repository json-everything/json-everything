using System.Text.Json.Nodes;
using Json.JsonE.Operators;

namespace Json.JsonE.Expressions.Functions;

internal class UppercaseFunction : FunctionDefinition
{
	private const string _name = "uppercase";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));

		return str.ToUpperInvariant();
	}
}