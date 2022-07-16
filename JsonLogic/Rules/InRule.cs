using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `in` operation.
/// </summary>
[Operator("in")]
public class InRule : Rule
{
	private readonly Rule _test;
	private readonly Rule _source;

	internal InRule(Rule test, Rule source)
	{
		_test = test;
		_source = source;
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
		var test = _test.Apply(data, contextData);
		var source = _source.Apply(data, contextData);

		if (source is JsonValue value && value.TryGetValue(out string? stringSource))
		{
			var stringTest = test.Stringify();

			if (stringTest == null || stringSource == null)
				throw new JsonLogicException($"Cannot check string for {test.JsonType()}.");

			return !string.IsNullOrEmpty(stringTest) && stringSource.Contains(stringTest);
		}

		if (source is JsonArray arr)
			return arr.Any(i => i.IsEquivalentTo(test));

		return false;
	}
}