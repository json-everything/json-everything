using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `none` operation.
/// </summary>
[Operator("none")]
public class NoneRule : Rule
{
	private readonly Rule _input;
	private readonly Rule _rule;

	internal NoneRule(Rule input, Rule rule)
	{
		_input = input;
		_rule = rule;
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
		var input = _input.Apply(data, contextData);

		if (input is not JsonArray arr)
			throw new JsonLogicException("Input must evaluate to an array.");

		return !arr.Select(value => _rule.Apply(data, value))
			.Any(result => result.IsTruthy());
	}
}