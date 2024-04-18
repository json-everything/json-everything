using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `log` operation.
/// </summary>
[Operator("log")]
[JsonConverter(typeof(LogRuleJsonConverter))]
public class LogRule : Rule, IRule
{
	internal Rule Log { get; }

	internal LogRule(Rule log)
	{
		Log = log;
	}
	/// <summary>
	/// Creates a new instance for model-less processing.
	/// </summary>
	protected internal LogRule(){}

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
		var log = Log.Apply(data, contextData);

		Console.WriteLine(log);

		return log;
	}

	JsonNode? IRule.Apply(JsonNode? args, EvaluationContext context)
	{
		var log = JsonLogic.Apply(args, context);

		Console.WriteLine(log);

		return log;
	}
}

internal class LogRuleJsonConverter : WeaklyTypedJsonConverter<LogRule>
{
	public override LogRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule)
			: new[] { options.Read(ref reader, JsonLogicSerializerContext.Default.Rule)! };

		return new LogRule(parameters!.Length == 0
			? new LiteralRule("")
			: parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, LogRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("log");
		options.Write(writer, value.Log, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
