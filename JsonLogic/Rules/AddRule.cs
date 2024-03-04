using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `+` operation.
/// </summary>
[Operator("+")]
[JsonConverter(typeof(AddRuleJsonConverter))]
public class AddRule : Rule
{
	/// <summary>
	/// The sequence of values to add together.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="AddRule"/> when '+' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value, to which other values will be added to.</param>
	/// <param name="more">Sequence of values to add to the first value.</param>
	protected internal AddRule(Rule a, params Rule[] more)
	{
		Items = [a, .. more];
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
		decimal result = 0;

		foreach (var item in Items)
		{
			var value = item.Apply(data, contextData);

			var number = value.Numberify();

			if (number == null) return null;

			result += number.Value;
		}

		return result;
	}
}

internal class AddRuleJsonConverter : WeaklyTypedJsonConverter<AddRule>
{
	public override AddRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule)
			: new[] { options.Read(ref reader, JsonLogicSerializerContext.Default.Rule)! };

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The + rule needs an array of parameters.");

		return new AddRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, AddRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("+");
		options.WriteList(writer, value.Items, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
