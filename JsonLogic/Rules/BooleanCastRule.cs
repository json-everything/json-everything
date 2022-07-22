using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!!` operation.
/// </summary>
[Operator("!!")]
[JsonConverter(typeof(BooleanCastRuleJsonConverter))]
public class BooleanCastRule : Rule
{
	private readonly Rule _value;

	internal BooleanCastRule(Rule value)
	{
		_value = value;
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
		var value = _value.Apply(data, contextData);

		return _value.Apply(data, contextData).IsTruthy();
	}
}

internal class BooleanCastRuleJsonConverter : JsonConverter<BooleanCastRule>
{
	public override BooleanCastRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 1 })
			throw new JsonException("The !! rule needs an array with a single parameter.");

		return new BooleanCastRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, BooleanCastRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
