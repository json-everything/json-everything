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
		var input = Input is LiteralRule { Value: null }
			? "null"
			: Input.Apply(data, contextData).Stringify() ?? string.Empty;
		var start = Start.Apply(data, contextData).Numberify() ?? 0;

		var numberStart = (int)start;

		if (numberStart < -input.Length) return input;
		if (numberStart < 0)
			numberStart = Math.Max(input.Length + numberStart, 0);
		if (numberStart >= input.Length) return string.Empty;

		if (Count == null) return input[numberStart..];

		var count = Count.Apply(data, contextData).Numberify() ?? 0;

		var integerCount = (int)count;
		var availableLength = input.Length - numberStart;
		if (integerCount < 0)
			integerCount = Math.Max(availableLength + integerCount, 0);
		integerCount = Math.Min(availableLength, integerCount);

		return input.Substring(numberStart, integerCount);
	}
}

internal class SubstrRuleJsonConverter : WeaklyTypedJsonConverter<SubstrRule>
{
	public override SubstrRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

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
		options.Write(writer, value.Input, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.Start, JsonLogicSerializerContext.Default.Rule);
		if (value.Count != null)
			options.Write(writer, value.Count, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
