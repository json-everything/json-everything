using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class BetweenExclusiveComponent : LogicComponent
	{
		private readonly LogicComponent _low;
		private readonly LogicComponent _value;
		private readonly LogicComponent _high;

		public BetweenExclusiveComponent(LogicComponent low, LogicComponent value, LogicComponent high)
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
			return (low.GetDecimal() < val && val < high.GetDecimal()).AsJsonElement();
		}
	}
}