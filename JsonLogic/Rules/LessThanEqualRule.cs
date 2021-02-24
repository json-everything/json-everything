using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("<=")]
	internal class LessThanEqualRule : Rule
	{
		private readonly Rule _a;
		private readonly Rule _b;
		private readonly Rule? _c;

		public LessThanEqualRule(Rule a, Rule b)
		{
			_a = a;
			_b = b;
		}
		public LessThanEqualRule(Rule a, Rule b, Rule c)
		{
			_a = a;
			_b = b;
			_c = c;
		}
	
		public override JsonElement Apply(JsonElement data)
		{
			if (_c == null)
			{
				var a = _a.Apply(data);
				var b = _b.Apply(data);

				var numberA = a.Numberify();
				var numberB = b.Numberify();

				if (numberA == null || numberB == null)
					throw new JsonLogicException($"Cannot compare {a.ValueKind} and {b.ValueKind}.");

				return (numberA <= numberB).AsJsonElement();
			}
			
			var low = _a.Apply(data);
			var value = _b.Apply(data);
			var high = _c.Apply(data);

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