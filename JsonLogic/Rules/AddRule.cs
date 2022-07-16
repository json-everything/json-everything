using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `+` operation.
/// </summary>
[Operator("+")]
public class AddRule : Rule
{
	private readonly List<Rule> _items;

	internal AddRule(Rule a, params Rule[] more)
	{
		_items = new List<Rule> { a };
		_items.AddRange(more);
	}

	/// <summary>
	/// Applies the rule to the input data.
	/// </summary>
	/// <param name="data">The input data.</param>
	/// <param name="contextData">
	///     Optional secondary data.  Used by a few operators to pass a secondary
	///     data context to inner operators.
	/// </param>
	/// <returns>The result of the rule.</returns>
	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		decimal result = 0;

		foreach (var item in _items)
		{
			var value = item.Apply(data, contextData);

			var number = value.Numberify();

			if (number == null)
				throw new JsonLogicException($"Cannot add {value.JsonType()}.");

			result += number.Value;
		}

		return result;
	}
}