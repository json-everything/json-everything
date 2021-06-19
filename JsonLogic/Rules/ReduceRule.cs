using System.Text.Json;

namespace Json.Logic.Rules
{
	[Operator("reduce")]
	internal class ReduceRule : Rule
	{
		private class Intermediary
		{
			public JsonElement Current { get; set; }
			public JsonElement Accumulator { get; set; }
		}

		private static readonly JsonSerializerOptions _options = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

		private readonly Rule _input;
		private readonly Rule _rule;
		private readonly Rule _initial;

		public ReduceRule(Rule input, Rule rule, Rule initial)
		{
			_input = input;
			_rule = rule;
			_initial = initial;
		}
		
		public override JsonElement Apply(JsonElement data)
		{
			var input = _input.Apply(data);
			var accumulator = _initial.Apply(data);

			if (input.ValueKind == JsonValueKind.Null) return accumulator;
			if (input.ValueKind != JsonValueKind.Array)
				throw new JsonLogicException($"Cannot reduce on {input.ValueKind}.");

			foreach (var element in input.EnumerateArray())
			{
				var intermediary = new Intermediary
				{
					Current = element,
					Accumulator = accumulator
				};
				using var doc = JsonDocument.Parse(JsonSerializer.Serialize(intermediary, _options));
				var item = doc.RootElement.Clone();
				
				accumulator = _rule.Apply(item);
			}

			return accumulator;
		}
	}
}