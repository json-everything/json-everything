using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!` operation.
/// </summary>
[Operator("!")]
[JsonConverter(typeof(NotRuleJsonConverter))]
public class NotRule : Rule
{
	/// <summary>
	/// The value to test.
	/// </summary>
	protected internal Rule Value { get; }

	/// <summary>
	/// Creates a new instance of <see cref="NotRule"/> when '!' operator is detected within json logic.
	/// </summary>
	/// <param name="value">The value to test.</param>
	protected internal NotRule(Rule value)
	{
		Value = value;
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
		var value = Value.Apply(data, contextData);

		return !value.IsTruthy();
	}
}

internal class NotRuleJsonConverter : WeaklyTypedJsonConverter<NotRule>
{
	public override NotRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule)
			: new[] { options.Read(ref reader, JsonLogicSerializerContext.Default.Rule)! };

		if (parameters is not { Length: 1 })
			throw new JsonException("The ! rule needs an array with a single parameter.");

		return new NotRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, NotRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("!");
		options.Write(writer, value.Value, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
