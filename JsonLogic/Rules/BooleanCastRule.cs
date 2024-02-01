using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `!!` operation.
/// </summary>
[Operator("!!")]
[JsonConverter(typeof(BooleanCastRuleJsonConverter))]
public class BooleanCastRule : Rule
{
	/// <summary>
	/// The value to test.
	/// </summary>
	protected internal Rule Value { get; }

	/// <summary>
	/// Creates a new instance of <see cref="BooleanCastRule"/> when '!!' operator is detected within json logic.
	/// </summary>
	/// <param name="value">The value to test.</param>
	protected internal BooleanCastRule(Rule value)
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
		return Value.Apply(data, contextData).IsTruthy();
	}
}

internal class BooleanCastRuleJsonConverter : AotCompatibleJsonConverter<BooleanCastRule>
{
	public override BooleanCastRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.Read(ref reader, JsonLogicSerializerContext.Default.RuleArray);

		if (parameters is not { Length: 1 })
			throw new JsonException("The !! rule needs an array with a single parameter.");

		return new BooleanCastRule(parameters[0]);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override void Write(Utf8JsonWriter writer, BooleanCastRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("!!");
		writer.WriteStartArray();
		writer.WriteRule(value.Value, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
