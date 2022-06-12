using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

[Operator("merge")]
internal class MergeRule : Rule
{
	private readonly List<Rule> _items;

	public MergeRule(params Rule[] items)
	{
		_items = items.ToList();
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var items = _items.Select(i => i.Apply(data, contextData)).SelectMany(e => e.Flatten());

		return items.ToJsonArray();
	}
}