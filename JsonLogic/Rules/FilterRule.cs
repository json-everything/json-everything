using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

[Operator("filter")]
internal class FilterRule : Rule
{
	private readonly Rule _input;
	private readonly Rule _rule;

	public FilterRule(Rule input, Rule rule)
	{
		_input = input;
		_rule = rule;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var input = _input.Apply(data, contextData);

		if (input is not JsonArray arr)
			return new JsonArray();

		return arr.Where(i => _rule.Apply(data, i).IsTruthy()).ToJsonArray();
	}
}