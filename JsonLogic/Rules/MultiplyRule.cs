using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("*")]
internal class MultiplyRule : Rule
{
	private readonly List<Rule> _items;

	public MultiplyRule(Rule a, params Rule[] more)
	{
		_items = new List<Rule> { a };
		_items.AddRange(more);
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		decimal result = 1;

		foreach (var item in _items)
		{
			var value = item.Apply(data, contextData);

			var number = value.Numberify();

			if (number == null)
				throw new JsonLogicException($"Cannot multiply {value.JsonType()}.");

			result *= number.Value;
		}

		return result;
	}
}