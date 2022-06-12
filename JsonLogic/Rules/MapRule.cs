using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("map")]
internal class MapRule : Rule
{
	private readonly Rule _input;
	private readonly Rule _rule;

	public MapRule(Rule input, Rule rule)
	{
		_input = input;
		_rule = rule;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var input = _input.Apply(data, contextData);

		if (input is not JsonArray arr)
			return new JsonArray();

		return new JsonArray(arr.Select(i => _rule.Apply(data, i)).ToArray());
	}
}