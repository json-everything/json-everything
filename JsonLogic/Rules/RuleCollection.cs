using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

internal class RuleCollection : Rule
{
	private readonly IEnumerable<Rule> _rules;

	public RuleCollection(params Rule[] rules)
	{
		_rules = rules;
	}
	public RuleCollection(IEnumerable<Rule> rules)
	{
		_rules = rules;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		return new JsonArray(_rules.Select(x => x.Apply(data, contextData)).ToArray());
	}
}