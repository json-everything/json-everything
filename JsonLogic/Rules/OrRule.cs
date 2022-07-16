using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `or` operation.
/// </summary>
[Operator("or")]
public class OrRule : Rule
{
	private readonly List<Rule> _items;

	internal OrRule(Rule a, params Rule[] more)
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
		var items = _items.Select(i => i.Apply(data, contextData));
		JsonNode? first = false;
		foreach (var x in items)
		{
			first = x;
			if (x.IsTruthy()) break;
		}

		return first;
	}
}