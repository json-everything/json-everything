using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `reduce` operation.
/// </summary>
[Operator("reduce")]
[JsonConverter(typeof(ReduceRuleJsonConverter))]
public class ReduceRule : Rule
{
	private class Intermediary
	{
		public JsonNode? Current { get; set; }
		public JsonNode? Accumulator { get; set; }
	}

	private static readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

	private readonly Rule _input;
	private readonly Rule _rule;
	private readonly Rule _initial;

	internal ReduceRule(Rule input, Rule rule, Rule initial)
	{
		_input = input;
		_rule = rule;
		_initial = initial;
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
		var accumulator = _initial.Apply(data, contextData);

		if (input is null) return accumulator;
		if (input is not JsonArray arr)
			throw new JsonLogicException($"Cannot reduce on {input.JsonType()}.");

		foreach (var element in arr)
		{
			var intermediary = new Intermediary
			{
				Current = element,
				Accumulator = accumulator
			};
			var item = JsonSerializer.SerializeToNode(intermediary, _options);

			accumulator = _rule.Apply(data, item);
		}

		return accumulator;
	}
}

internal class ReduceRuleJsonConverter : JsonConverter<ReduceRule>
{
	public override ReduceRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 3 })
			throw new JsonException("The reduce rule needs an array with 3 parameters.");

		return new ReduceRule(parameters[0], parameters[1], parameters[2]);
	}

	public override void Write(Utf8JsonWriter writer, ReduceRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
