using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!=` operation.
/// </summary>
[Operator("!=")]
[JsonConverter(typeof(LooseNotEqualsRuleJsonConverter))]
public class LooseNotEqualsRule : Rule
{
	/// <summary>
	/// First value to compare.
	/// </summary>
	protected internal Rule A { get; }
	/// <summary>
	/// Second value to compare.
	/// </summary>
	protected internal Rule B { get; }

	/// <summary>
	/// Creates a new instance of <see cref="LooseNotEqualsRule"/> when '!=' operator is detected within json logic.
	/// </summary>
	/// <param name="a">First value to compare.</param>
	/// <param name="b">Second value to compare.</param>
	protected internal LooseNotEqualsRule(Rule a, Rule b)
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

internal class LooseNotEqualsRuleJsonConverter : WeaklyTypedJsonConverter<LooseNotEqualsRule>
{
	public override LooseNotEqualsRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not { Length: 2 })
			throw new JsonException("The != rule needs an array with 2 parameters.");

		return new LooseNotEqualsRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, LooseNotEqualsRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("!=");
		writer.WriteStartArray();
		options.Write(writer, value.A, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.B, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
