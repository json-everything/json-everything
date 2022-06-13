using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("reduce")]
internal class ReduceRule : Rule
{
	private class Intermediary
	{
		public JsonNode? Current { get; set; }
		public JsonNode? Accumulator { get; set; }
	}

	private static readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

	private readonly Rule _input;
	private readonly Rule _rule;
	private readonly Rule _initial;

	public ReduceRule(Rule input, Rule rule, Rule initial)
	{
		_input = input;
		_rule = rule;
		_initial = initial;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var input = _input.Apply(data, contextData);
		var accumulator = _initial.Apply(data, contextData);

		if (input is null) return accumulator;
		if (input is not JsonArray arr)
			throw new JsonLogicException($"Cannot reduce on {input.JsonType()}.");

		foreach (var element in arr)
		{
			var intermediary = new Intermediary
			{
				Current = element,
				Accumulator = accumulator
			};
			var item = JsonSerializer.SerializeToNode(intermediary, _options);

			accumulator = _rule.Apply(data, item);
		}

		return accumulator;
	}
}