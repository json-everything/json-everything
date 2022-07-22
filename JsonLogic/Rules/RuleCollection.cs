using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Provides a stand-in "rule" for collections of rules.
/// </summary>
/// <remarks>This is not exactly part of the specification, but it helps things in this library.</remarks>
public class RuleCollection : Rule
{
	private readonly IEnumerable<Rule> _rules;

	internal RuleCollection(params Rule[] rules)
	{
		_rules = rules;
	}
	internal RuleCollection(IEnumerable<Rule> rules)
	{
		_rules = rules;
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
		return _rules.Select(x => x.Apply(data, contextData)).ToJsonArray();
	}
}