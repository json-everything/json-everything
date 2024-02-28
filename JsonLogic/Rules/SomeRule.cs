using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `some` operation.
/// </summary>
[Operator("some")]
[JsonConverter(typeof(SomeRuleJsonConverter))]
public class SomeRule : Rule
{
	/// <summary>
	/// The sequence of elements to apply the rule to.
	/// </summary>
	protected internal Rule Input { get; }
	/// <summary>
	/// The rule to apply to items in the input sequence.
	/// </summary>
	protected internal Rule Rule { get; }

	/// <summary>
	/// Creates a new instance of <see cref="SomeRule"/> when 'some' operator is detected within json logic.
	/// </summary>
	/// <param name="input">A sequence of elements to apply the rule to.</param>
	/// <param name="rule">A rule to apply to items in the input sequence.</param>
	protected internal SomeRule(Rule input, Rule rule)
	{
		Input = input;
		Rule = rule;
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
		var input = Input.Apply(data, contextData);

		if (input is not JsonArray arr) return false;

		return arr.Select(value => Rule.Apply(contextData, value))
			.Any(result => result.IsTruthy());
	}
}

internal class SomeRuleJsonConverter : WeaklyTypedJsonConverter<SomeRule>
{
	public override SomeRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not { Length: 2 })
			throw new JsonException("The some rule needs an array with 2 parameters.");

		return new SomeRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, SomeRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("some");
		writer.WriteStartArray();
		options.Write(writer, value.Input, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.Rule, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
