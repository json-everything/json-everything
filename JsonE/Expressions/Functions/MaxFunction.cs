using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions.Functions;

internal class MaxFunction : FunctionDefinition
{
	public override string Name => "max";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		var nums = arguments.Select(x => (x as JsonValue)?.GetNumber()).ToArray();
		if (nums.Any(x => !x.HasValue))
			throw new BuiltInException(CommonErrors.IncorrectArgType(Name));

		return nums.Max();
	}
}