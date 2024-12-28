﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `*` operation.
/// </summary>
[Operator("*")]
[JsonConverter(typeof(MultiplyRuleJsonConverter))]
public class MultiplyRule : Rule, IRule
{
	/// <summary>
	/// The sequence of numbers to multiple together.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MultiplyRule"/> when '*' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value in to which other values will be multiplied by.</param>
	/// <param name="more">Sequence of values to multiply by.</param>
	protected internal MultiplyRule(Rule a, params Rule[] more)
	{
		Items = [a, .. more];
	}

	/// <summary>
	/// Creates a new instance of <see cref="MultiplyRule"/> when '*' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value in to which other values will be multiplied by.</param>
	/// <param name="more">Sequence of values to multiply by.</param>
	protected internal MultiplyRule(Rule a, ReadOnlySpan<Rule> more)
	{
		Items = [a, .. more];
	}

	/// <summary>
	/// Creates a new instance for model-less processing.
	/// </summary>
	protected internal MultiplyRule(){}

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

		foreach (var item in Items)
		{
			var value = item.Apply(data, contextData);

			var number = value.Numberify();

			if (number == null) return null;

			result *= number.Value;
		}

		return result;
	}

	JsonNode? IRule.Apply(JsonNode? args, EvaluationContext context)
	{
		if (args is not JsonArray { Count: > 0 } array)
			throw new JsonLogicException("The '*' rule needs an array of parameters");

		decimal? result = 1;
		foreach (var item in array)
		{
			var value = JsonLogic.Apply(item, context).Numberify();
			if (value is null) return null;

			result *= value;
		}

		return result;
	}
}

internal class MultiplyRuleJsonConverter : WeaklyTypedJsonConverter<MultiplyRule>
{
	public override MultiplyRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The * rule needs an array of parameters.");

		return new MultiplyRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, MultiplyRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("*");
		options.WriteList(writer, value.Items, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
