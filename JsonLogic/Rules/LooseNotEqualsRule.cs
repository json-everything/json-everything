using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!=` operation.
/// </summary>
[Operator("!=")]
[JsonConverter(typeof(LooseNotEqualsRuleJsonConverter))]
public class LooseNotEqualsRule : Rule
{
	internal Rule A { get; }
	internal Rule B { get; }

	internal LooseNotEqualsRule(Rule a, Rule b)
	{
		A = a;
		B = b;
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
		var a = A.Apply(data, contextData);
		var b = B.Apply(data, contextData);

		return !a.LooseEquals(b);
	}
}

internal class LooseNotEqualsRuleJsonConverter : JsonConverter<LooseNotEqualsRule>
{
	public override LooseNotEqualsRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 2 })
			throw new JsonException("The != rule needs an array with 2 parameters.");

		return new LooseNotEqualsRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, LooseNotEqualsRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("!=");
		writer.WriteStartArray();
		writer.WriteRule(value.A, options);
		writer.WriteRule(value.B, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
