using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `none` operation.
/// </summary>
[Operator("none")]
[JsonConverter(typeof(NoneRuleJsonConverter))]
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

internal class NoneRuleJsonConverter : JsonConverter<NoneRule>
{
	public override NoneRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 2 })
			throw new JsonException("The none rule needs an array with 2 parameters.");

		return new NoneRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, NoneRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
