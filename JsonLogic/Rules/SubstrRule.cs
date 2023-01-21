using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `substr` operation.
/// </summary>
[Operator("substr")]
[JsonConverter(typeof(SubstrRuleJsonConverter))]
public class SubstrRule : Rule
{
	/// <summary>
	/// The input string.
	/// </summary>
	protected internal Rule Input { get; }
	/// <summary>
	/// The positive start position within the input string.
	/// </summary>
	protected internal Rule Start { get; }
	/// <summary>
	/// How many characters to return.
	/// </summary>
	protected internal Rule? Count { get; }

	/// <summary>
	/// Creates a new instance of <see cref="SubstrRule"/> when 'substr' operator is detected within json logic.
	/// </summary>
	/// <param name="input">The input string.</param>
	/// <param name="start">The positive start position within the input string.</param>
	protected internal SubstrRule(Rule input, Rule start)
	{
		Input = input;
		Start = start;
	}

	/// <summary>
	/// Creates a new instance of <see cref="SubstrRule"/> when 'substr' operator is detected within json logic.
	/// </summary>
	/// <param name="input">The input string.</param>
	/// <param name="start">The positive start position within the input string.</param>
	/// <param name="count">How many characters to return.</param>
	protected internal SubstrRule(Rule input, Rule start, Rule count)
	{
		Input = input;
		Start = start;
		Count = count;
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
		var start = Start.Apply(data, contextData);

		if (input is not JsonValue inputValue || !inputValue.TryGetValue(out string? stringInput))
			throw new JsonLogicException($"Cannot substring a {input.JsonType()}.");

		if (start is not JsonValue startValue || startValue.GetInteger() == null)
			throw new JsonLogicException("Start value must be an integer");

		var numberStart = (int)startValue.GetInteger()!.Value;

		if (numberStart < -stringInput.Length) return input;
		if (numberStart < 0)
			numberStart = Math.Max(stringInput.Length + numberStart, 0);
		if (numberStart >= stringInput.Length) return string.Empty;

		if (Count == null) return stringInput[numberStart..];

		var count = Count.Apply(data, contextData);
		if (count is not JsonValue countValue || countValue.GetInteger() == null)
			throw new JsonLogicException("Count value must be an integer");

		var integerCount = (int)countValue.GetInteger()!.Value;
		var availableLength = stringInput.Length - numberStart;
		if (integerCount < 0)
			integerCount = Math.Max(availableLength + integerCount, 0);
		integerCount = Math.Min(availableLength, integerCount);

		return stringInput.Substring(numberStart, integerCount);
	}
}

internal class SubstrRuleJsonConverter : JsonConverter<SubstrRule>
{
	public override SubstrRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not ({ Length: 2 } or { Length: 3 }))
			throw new JsonException("The substr rule needs an array with either 2 or 3 parameters.");

		if (parameters.Length == 2) return new SubstrRule(parameters[0], parameters[1]);

		return new SubstrRule(parameters[0], parameters[1], parameters[2]);
	}

	public override void Write(Utf8JsonWriter writer, SubstrRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("substr");
		writer.WriteStartArray();
		writer.WriteRule(value.Input, options);
		writer.WriteRule(value.Start, options);
		if (value.Count != null)
			writer.WriteRule(value.Count, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
