using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `%` operation.
/// </summary>
[Operator("%")]
[JsonConverter(typeof(ModRuleJsonConverter))]
public class ModRule : Rule
{
	/// <summary>
	/// The value to divide.
	/// </summary>
	protected internal Rule A { get; }
	/// <summary>
	/// The divisor, ie; the value to divide by.
	/// </summary>
	protected internal Rule B { get; }

	/// <summary>
	/// Creates a new instance of <see cref="ModRule"/> when '%' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The value to divide.</param>
	/// <param name="b">The divisor, ie; the value to divide by.</param>
	protected internal ModRule(Rule a, Rule b)
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

		var numberA = a.Numberify();
		var numberB = b.Numberify();

		if (numberA == null || numberB == null) return null;

		if (numberB == 0) return null;

		return numberA.Value % numberB.Value;
	}
}

internal class ModRuleJsonConverter : WeaklyTypedJsonConverter<ModRule>
{
	public override ModRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not { Length: 2 })
			throw new JsonException("The % rule needs an array with 2 parameters.");

		return new ModRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, ModRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("%");
		writer.WriteStartArray();
		options.Write(writer, value.A, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.B, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
