using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions.Functions;

internal class MaxFunction : FunctionDefinition
{
	private const string _name = "max";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments.Length == 0)
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name, "expected at least 1 arguments"));

		var nums = arguments.Select(x => (x as JsonValue)?.GetNumber()).ToArray();
		if (nums.Any(x => !x.HasValue))
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));

		return nums.Max();
	}
}