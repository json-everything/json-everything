using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `some` operation.
/// </summary>
[Operator("some")]
[JsonConverter(typeof(SomeRuleJsonConverter))]
public class SomeRule : Rule
{
	internal Rule Input { get; }
	internal Rule Rule { get; }

	internal SomeRule(Rule input, Rule rule)
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

		return arr.Select(value => Rule.Apply(data, value))
			.Any(result => result.IsTruthy());
	}
}

internal class SomeRuleJsonConverter : JsonConverter<SomeRule>
{
	public override SomeRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 2 })
			throw new JsonException("The some rule needs an array with 2 parameters.");

		return new SomeRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, SomeRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("some");
		writer.WriteStartArray();
		writer.WriteRule(value.Input, options);
		writer.WriteRule(value.Rule, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
