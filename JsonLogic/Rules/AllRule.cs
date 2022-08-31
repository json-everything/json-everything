using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `all` operation.
/// </summary>
[Operator("all")]
[JsonConverter(typeof(AllRuleJsonConverter))]
public class AllRule : Rule
{
	internal Rule Input { get; }
	internal Rule Rule { get; }

	internal AllRule(Rule input, Rule rule)
	{
		Input = input;
		Rule = rule;
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
		var input = Input.Apply(data, contextData);

		if (input is not JsonArray arr)
			throw new JsonLogicException("Input must evaluate to an array.");

		var results = arr.Select(value => Rule.Apply(data, value)).ToList();
		return (results.Any() &&
				results.All(result => result.IsTruthy()));
	}
}

internal class AllRuleJsonConverter : JsonConverter<AllRule>
{
	public override AllRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length:2})
			throw new JsonException("The all rule needs an array with 2 parameters.");

		return new AllRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, AllRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("all");
		writer.WriteStartArray();
		writer.WriteRule(value.Input, options);
		writer.WriteRule(value.Rule, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
