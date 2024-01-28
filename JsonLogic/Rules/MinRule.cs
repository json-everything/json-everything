using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

// ReSharper disable PossibleMultipleEnumeration

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `min` operation.
/// </summary>
[Operator("min")]
[JsonConverter(typeof(MinRuleJsonConverter))]
public class MinRule : Rule
{
	/// <summary>
	/// The sequence of numbers to query for min.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MinRule"/> when 'min' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first numeric value in a sequence of numbers.</param>
	/// <param name="more">A sequence of numbers.</param>
	protected internal MinRule(Rule a, params Rule[] more)
	{
		Items = new List<Rule> { a };
		Items.AddRange(more);
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
		var items = Items.Select(i => i.Apply(data, contextData)).Select(e => new { Type = e.JsonType(), Value = e.Numberify() }).ToList();
		var nulls = items.Where(i => i.Value == null);
		if (nulls.Any()) return null;

		return items.Min(i => i.Value!.Value);
	}
}

internal class MinRuleJsonConverter : JsonConverter<MinRule>
{
	public override MinRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The min rule needs an array of parameters.");

		return new MinRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, MinRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("min");
		writer.WriteRules(value.Items, options);
		writer.WriteEndObject();
	}
}
