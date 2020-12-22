using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("reduce")]
	internal class ReduceComponent : LogicComponent
	{
		private class Intermediary
		{
			public JsonElement Current { get; set; }
			public JsonElement Accumulator { get; set; }
		}

		private static readonly JsonSerializerOptions _options = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

		private readonly LogicComponent _input;
		private readonly LogicComponent _rule;
		private readonly LogicComponent _initial;

		public ReduceComponent(LogicComponent input, LogicComponent rule, LogicComponent initial)
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
				var item = JsonDocument.Parse(JsonSerializer.Serialize(intermediary, _options)).RootElement;
				
				accumulator = _rule.Apply(item);
			}

			return accumulator;
		}
	}
}