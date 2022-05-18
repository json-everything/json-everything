using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

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

	public override JsonElement Apply(JsonElement data, JsonElement? contextData = null)
	{
		return _rules.Select(x => x.Apply(data, contextData)).AsJsonElement();
	}
}