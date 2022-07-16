using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!==` operation.
/// </summary>
[Operator("!==")]
public class StrictNotEqualsRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;

	internal StrictNotEqualsRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
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
		return !_a.Apply(data, contextData).IsEquivalentTo(_b.Apply(data, contextData));
	}
}