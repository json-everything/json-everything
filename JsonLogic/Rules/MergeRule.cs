using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `merge` operation.
/// </summary>
[Operator("merge")]
public class MergeRule : Rule
{
	private readonly List<Rule> _items;

	internal MergeRule(params Rule[] items)
	{
		_items = items.ToList();
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
		var items = _items.Select(i => i.Apply(data, contextData)).SelectMany(e => e.Flatten());

		return items.ToJsonArray();
	}
}