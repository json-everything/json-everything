using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions.Functions;

internal class StrFunction : FunctionDefinition
{
	public override string Name => "str";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var num = (arguments[0] as JsonValue)?.GetNumber();
		if (!num.HasValue)
			throw new BuiltInException(CommonErrors.IncorrectArgType(Name));

		return num.ToString();
	}
}