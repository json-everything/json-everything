using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `map` operation.
/// </summary>
[Operator("map")]
[JsonConverter(typeof(MapRuleJsonConverter))]
public class MapRule : Rule
{
	/// <summary>
	/// A sequence of values to map over.
	/// </summary>
	protected internal Rule Input { get; }
	/// <summary>
	/// A rule to apply to each item in the sequence.
	/// </summary>
	protected internal Rule Rule { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MapRule"/> when 'map' operator is detected within json logic.
	/// </summary>
	/// <param name="input">A sequence of values to map over.</param>
	/// <param name="rule">A rule to apply to each item in the sequence.</param>
	protected internal MapRule(Rule input, Rule rule)
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
			return new JsonArray();

		return arr.Select(i => Rule.Apply(data, i)).ToJsonArray();
	}
}

internal class MapRuleJsonConverter : WeaklyTypedJsonConverter<MapRule>
{
	public override MapRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not { Length: 2 })
			throw new JsonException("The map rule needs an array with 2 parameters.");

		return new MapRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, MapRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("map");
		writer.WriteStartArray();
		options.Write(writer, value.Input, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.Rule, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
