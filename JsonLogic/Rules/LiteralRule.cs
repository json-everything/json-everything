using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Provides a stand-in "rule" for literal values.
/// </summary>
/// <remarks>This is not exactly part of the specification, but it helps things in this library.</remarks>
[Operator("")]
public class LiteralRule : Rule
{
	private readonly JsonNode? _value;

	internal static readonly LiteralRule Null = new(null);

	internal LiteralRule(JsonNode? value)
	{
		_value = ReferenceEquals(JsonNull.SignalNode, value) ? null : value.Copy();
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
		return _value;
	}
}