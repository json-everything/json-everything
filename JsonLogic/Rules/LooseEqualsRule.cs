using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `==` operation.
/// </summary>
[Operator("==")]
[JsonConverter(typeof(LooseEqualsRuleJsonConverter))]
public class LooseEqualsRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;

	internal LooseEqualsRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
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
		var a = _a.Apply(data, contextData);
		var b = _b.Apply(data, contextData);

		return a.LooseEquals(b);
	}
}

internal class LooseEqualsRuleJsonConverter : JsonConverter<LooseEqualsRule>
{
	public override LooseEqualsRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 2 })
			throw new JsonException("The == rule needs an array with 2 parameters.");

		return new LooseEqualsRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, LooseEqualsRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
