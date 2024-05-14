using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `-` operation.
/// </summary>
[Operator("-")]
[JsonConverter(typeof(SubtractRuleJsonConverter))]
public class SubtractRule : Rule, IRule
{
	/// <summary>
	/// The sequence of values to subtract.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="SubtractRule"/> when '-' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value, from which other values will be subtracted.</param>
	/// <param name="more">Sequence of values to subtract from the first value.</param>
	protected internal SubtractRule(Rule a, params Rule[] more)
	{
		Items = [a, .. more];
	}
	/// <summary>
	/// Creates a new instance for model-less processing.
	/// </summary>
	protected internal SubtractRule(){}

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
		if (Items.Count == 0) return 0;

		var value = Items[0].Apply(data, contextData);
		var number = value.Numberify();

		if (number == null) return null;

		var result = number.Value;

		if (Items.Count == 1) return -result;

		foreach (var item in Items.Skip(1))
		{
			value = item.Apply(data, contextData);

			number = value.Numberify();

			if (number == null) return null;

			result -= number.Value;
		}

		return result;
	}

	JsonNode? IRule.Apply(JsonNode? args, EvaluationContext context)
	{
		if (args is not JsonArray {Count: > 0} array)
			throw new JsonLogicException("The '-' rule needs an array of parameters");

		var value = JsonLogic.Apply(array[0], context).Numberify();
		if (value is null) return null;
		if (array.Count == 1) return -value;

		var result = value;
		foreach (var item in array.Skip(1))
		{
			value = JsonLogic.Apply(item, context).Numberify();
			if (value is null) return null;

			result -= value;
		}

		return result;
	}
}

internal class SubtractRuleJsonConverter : WeaklyTypedJsonConverter<SubtractRule>
{
	public override SubtractRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The - rule needs an array of parameters.");

		return new SubtractRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, SubtractRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("-");
		options.WriteList(writer, value.Items, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
