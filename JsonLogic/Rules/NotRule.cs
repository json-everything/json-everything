using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!` operation.
/// </summary>
[Operator("!")]
[JsonConverter(typeof(NotRuleJsonConverter))]
public class NotRule : Rule
{
	internal Rule Value { get; }

	internal NotRule(Rule value)
	{
		Value = value;
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
		var value = Value.Apply(data, contextData);

		return !value.IsTruthy();
	}
}

internal class NotRuleJsonConverter : JsonConverter<NotRule>
{
	public override NotRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode?>(ref reader, options);
	
		var parameters = node is JsonArray
			? node.Deserialize<Rule[]>()
			: new[] { node.Deserialize<Rule>()! };

		if (parameters is not { Length: 1 })
			throw new JsonException("The ! rule needs an array with a single parameter.");

		return new NotRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, NotRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("!");
		writer.WriteStartArray();
		writer.WriteRule(value.Value, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
