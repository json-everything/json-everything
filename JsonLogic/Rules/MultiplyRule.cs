using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `*` operation.
/// </summary>
[Operator("*")]
[JsonConverter(typeof(MultiplyRuleJsonConverter))]
public class MultiplyRule : Rule
{
	private readonly List<Rule> _items;

	internal MultiplyRule(Rule a, params Rule[] more)
	{
		_items = new List<Rule> { a };
		_items.AddRange(more);
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
		decimal result = 1;

		foreach (var item in _items)
		{
			var value = item.Apply(data, contextData);

			var number = value.Numberify();

			if (number == null)
				throw new JsonLogicException($"Cannot multiply {value.JsonType()}.");

			result *= number.Value;
		}

		return result;
	}
}

internal class MultiplyRuleJsonConverter : JsonConverter<MultiplyRule>
{
	public override MultiplyRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The * rule needs an array of parameters.");

		return new MultiplyRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, MultiplyRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
