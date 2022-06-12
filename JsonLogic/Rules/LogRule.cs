using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("log")]
internal class LogRule : Rule
{
	private readonly Rule _log;

	public LogRule(Rule log)
	{
		_log = log;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var log = _log.Apply(data, contextData);

		Console.WriteLine(log);

		return data;
	}
}