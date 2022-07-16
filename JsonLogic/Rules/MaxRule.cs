using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

// ReSharper disable PossibleMultipleEnumeration

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `max` operation.
/// </summary>
[Operator("max")]
public class MaxRule : Rule
{
	private readonly List<Rule> _items;

	internal MaxRule(Rule a, params Rule[] more)
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
		var items = _items.Select(i => i.Apply(data, contextData))
			.Select(e => new { Type = e.JsonType(), Value = e.Numberify() })
			.ToList();
		var nulls = items.Where(i => i.Value == null);
		if (nulls.Any())
			throw new JsonLogicException($"Cannot find max with {nulls.First().Type}.");

		return items.Max(i => i.Value!.Value);
	}
}