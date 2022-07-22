using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
#pragma warning disable CS1570

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `<` operation.
/// </summary>
[Operator("<")]
[JsonConverter(typeof(LessThanRuleJsonConverter))]
public class LessThanRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;
	private readonly Rule? _c;

	internal LessThanRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
	}

	internal LessThanRule(Rule a, Rule b, Rule c)
	{
		_a = a;
		_b = b;
		_c = c;
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
		if (_c == null)
		{
			var a = _a.Apply(data, contextData);
			var b = _b.Apply(data, contextData);

			var numberA = a.Numberify();
			var numberB = b.Numberify();

			if (numberA == null || numberB == null)
				throw new JsonLogicException($"Cannot compare {a.JsonType()} and {b.JsonType()}.");

			return numberA < numberB;
		}

		var low = (_a.Apply(data, contextData) as JsonValue)?.GetNumber();
		if (low == null)
			throw new JsonLogicException("Lower bound must be a number.");

		var value = (_b.Apply(data, contextData) as JsonValue)?.GetNumber();
		if (value == null)
			throw new JsonLogicException("Value must be a number.");

		var high = (_c.Apply(data, contextData) as JsonValue)?.GetNumber();
		if (high == null)
			throw new JsonLogicException("Upper bound must be a number.");

		return low < value && value < high;
	}
}

internal class LessThanRuleJsonConverter : JsonConverter<LessThanRule>
{
	public override LessThanRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not ({ Length: 2 } or { Length: 3 }))
			throw new JsonException("The < rule needs an array with either 2 or 3 parameters.");

		if (parameters.Length == 2) return new LessThanRule(parameters[0], parameters[1]);

		return new LessThanRule(parameters[0], parameters[1], parameters[2]);
	}

	public override void Write(Utf8JsonWriter writer, LessThanRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
