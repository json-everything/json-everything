using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

	public override JsonElement Apply(JsonElement data, JsonElement? contextData = null)
	{
		var items = _items.Select(i => i.Apply(data, contextData)).SelectMany(e => e.Flatten());

		return items.AsJsonElement();
	}
}