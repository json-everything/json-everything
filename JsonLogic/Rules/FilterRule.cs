using System;
using System.Linq;
using System.Text.Json;
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

	public override JsonElement Apply(JsonElement data, JsonElement? contextData = null)
	{
		var input = _input.Apply(data, contextData);

		if (input.ValueKind != JsonValueKind.Array)
			return Array.Empty<JsonElement>().AsJsonElement();

		return input.EnumerateArray().Where(i => _rule.Apply(data, i).IsTruthy()).AsJsonElement();
	}
}