using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!!` operation.
/// </summary>
[Operator("!!")]
public class BooleanCastRule : Rule
{
	private readonly Rule _value;

	internal BooleanCastRule(Rule value)
	{
		_value = value;
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
		var value = _value.Apply(data, contextData);

		return _value.Apply(data, contextData).IsTruthy();
	}
}