using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("between_ex")]
	internal class BetweenInclusiveRule : Rule
	{
		private readonly Rule _low;
		private readonly Rule _value;
		private readonly Rule _high;

		public BetweenInclusiveRule(Rule low, Rule value, Rule high)
		{
			_low = low;
			_value = value;
			_high = high;
		}
	
		public override JsonElement Apply(JsonElement data)
		{
			var low = _low.Apply(data);
			var value = _value.Apply(data);
			var high = _high.Apply(data);
			
			if (low.ValueKind != JsonValueKind.Number)
				throw new JsonLogicException("Lower bound must be a number.");
			if (value.ValueKind != JsonValueKind.Number)
				throw new JsonLogicException("Value must be a number.");
			if (high.ValueKind != JsonValueKind.Number)
				throw new JsonLogicException("Upper bound must be a number.");

			var val = value.GetDecimal();
			return (low.GetDecimal() <= val && val <= high.GetDecimal()).AsJsonElement();
		}
	}
}