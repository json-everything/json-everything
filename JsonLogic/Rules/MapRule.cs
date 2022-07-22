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
	private readonly Rule _input;
	private readonly Rule _rule;

	internal MapRule(Rule input, Rule rule)
	{
		_input = input;
		_rule = rule;
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
		var input = _input.Apply(data, contextData);

		if (input is not JsonArray arr)
			return new JsonArray();

		return arr.Select(i => _rule.Apply(data, i)).ToJsonArray();
	}
}

internal class MapRuleJsonConverter : JsonConverter<MapRule>
{
	public override MapRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 2 })
			throw new JsonException("The map rule needs an array with 2 parameters.");

		return new MapRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, MapRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
