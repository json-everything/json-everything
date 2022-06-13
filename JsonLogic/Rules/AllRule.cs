using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("all")]
internal class AllRule : Rule
{
	private readonly Rule _input;
	private readonly Rule _rule;

	public AllRule(Rule input, Rule rule)
	{
		_input = input;
		_rule = rule;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var input = _input.Apply(data, contextData);

		if (input is not JsonArray arr)
			throw new JsonLogicException("Input must evaluate to an array.");

		var results = arr.Select(value => _rule.Apply(data, value)).ToList();
		return (results.Any() &&
				results.All(result => result.IsTruthy()));
	}
}