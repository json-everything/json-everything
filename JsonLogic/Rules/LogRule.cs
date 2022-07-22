using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `log` operation.
/// </summary>
[Operator("log")]
[JsonConverter(typeof(LogRuleJsonConverter))]
public class LogRule : Rule
{
	private readonly Rule _log;

	internal LogRule(Rule log)
	{
		_log = log;
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
		var log = _log.Apply(data, contextData);

		Console.WriteLine(log);

		return data;
	}
}

internal class LogRuleJsonConverter : JsonConverter<LogRule>
{
	public override LogRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 1 })
			throw new JsonException("The log rule needs an array with a single parameter.");

		return new LogRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, LogRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
