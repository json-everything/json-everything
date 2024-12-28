using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `and` operation.
/// </summary>
[Operator("and")]
[JsonConverter(typeof(AndRuleJsonConverter))]
public class AndRule : Rule, IRule
{
	/// <summary>
	/// The sequence of values to And against.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="AndRule"/> when 'and' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value.</param>
	/// <param name="more">Sequence of values to And against.</param>
	protected internal AndRule(Rule a, params Rule[] more)
	{
		Items = [a, .. more];
	}

	/// <summary>
	/// Creates a new instance of <see cref="AndRule"/> when 'and' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value.</param>
	/// <param name="more">Sequence of values to And against.</param>
	protected internal AndRule(Rule a, ReadOnlySpan<Rule> more)
	{
		Items = [a, .. more];
	}

	/// <summary>
	/// Creates a new instance for model-less processing.
	/// </summary>
	protected internal AndRule(){}

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
		var items = Items.Select(i => i.Apply(data, contextData));
		JsonNode? first = false;
		foreach (var x in items)
		{
			first = x;
			if (!x.IsTruthy()) break;
		}

		return first;
	}

	JsonNode? IRule.Apply(JsonNode? args, EvaluationContext context)
	{
		if (args is not JsonArray array)
			throw new JsonLogicException("The 'and' rule requires an array of arguments");

		if (array.Count == 0) return false;

		JsonNode? result = null;
		foreach (var item in array)
		{
			result = item is JsonObject innerRule ? JsonLogic.Apply(innerRule, context) : item;
			if (!result.IsTruthy()) break;
		}

		return result;
	}
}

internal class AndRuleJsonConverter : WeaklyTypedJsonConverter<AndRule>
{
	public override AndRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The and rule needs an array of parameters.");

		return new AndRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, AndRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("and");
		options.WriteList(writer, value.Items, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
