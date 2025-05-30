﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

// ReSharper disable PossibleMultipleEnumeration

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `max` operation.
/// </summary>
[Operator("max")]
[JsonConverter(typeof(MaxRuleJsonConverter))]
public class MaxRule : Rule, IRule
{
	/// <summary>
	/// The sequence of numbers to query for max.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MaxRule"/> when 'max' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first numeric value in a sequence of numbers.</param>
	/// <param name="more">A sequence of numbers.</param>
	protected internal MaxRule(Rule a, params Rule[] more)
	{
		Items = [a, .. more];
	}

	/// <summary>
	/// Creates a new instance of <see cref="MaxRule"/> when 'max' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first numeric value in a sequence of numbers.</param>
	/// <param name="more">A sequence of numbers.</param>
	protected internal MaxRule(Rule a, ReadOnlySpan<Rule> more)
	{
		Items = [a, .. more];
	}

	/// <summary>
	/// Creates a new instance for model-less processing.
	/// </summary>
	protected internal MaxRule(){}

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
		var items = Items.Select(i => i.Apply(data, contextData))
			.Select(e => new { Type = e.JsonType(), Value = e.Numberify() })
			.ToList();
		var nulls = items.Where(i => i.Value == null);
		if (nulls.Any()) return null;

		return items.Max(i => i.Value!.Value);
	}

	JsonNode? IRule.Apply(JsonNode? args, EvaluationContext context)
	{
		if (args is not JsonArray array)
			return JsonLogic.Apply(args, context).Numberify();

		if (array.Count == 0) return null;

		var result = array[0].Numberify();
		if (result is null) return null;

		foreach (var item in array.Skip(1))
		{
			var value = JsonLogic.Apply(item, context);
			var number = value.Numberify();
			if (number is null) return null;

			result = Math.Max(result.Value, number.Value);
		}

		return result;
	}
}

internal class MaxRuleJsonConverter : WeaklyTypedJsonConverter<MaxRule>
{
	public override MaxRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The max rule needs an array of parameters.");

		return new MaxRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, MaxRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("max");
		options.WriteList(writer, value.Items, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
