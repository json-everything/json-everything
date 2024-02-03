using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Provides a stand-in "rule" for literal values.
/// </summary>
/// <remarks>This is not exactly part of the specification, but it helps things in this library.</remarks>
[Operator("")]
[JsonConverter(typeof(LiteralRuleJsonConverter))]
public class LiteralRule : Rule
{
	internal JsonNode? Value { get; }

	internal static readonly LiteralRule Null = new(null);

	internal LiteralRule(JsonNode? value)
	{
		Value = value?.DeepClone();
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
		return Value;
	}
}

internal class LiteralRuleJsonConverter : WeaklyTypedJsonConverter<LiteralRule>
{
	public override LiteralRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// this is handled by Rule
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, LiteralRule value, JsonSerializerOptions options)
	{
		if (value.Value is null) 
			writer.WriteNullValue();
		else
			JsonSerializer.Serialize(writer, value.Value, JsonLogicSerializerContext.Default.JsonNode!);
	}
}