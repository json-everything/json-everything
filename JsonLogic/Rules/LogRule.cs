using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `log` operation.
/// </summary>
[Operator("log")]
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