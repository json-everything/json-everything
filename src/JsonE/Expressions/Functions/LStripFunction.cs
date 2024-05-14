using System.Text.Json.Nodes;
using Json.JsonE.Operators;

namespace Json.JsonE.Expressions.Functions;

internal class LStripFunction : FunctionDefinition
{
	private const string _name = "lstrip";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new InterpreterException(CommonErrors.IncorrectArgType(_name));

		return str.TrimStart();
	}
}