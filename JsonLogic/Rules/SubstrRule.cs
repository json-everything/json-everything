using System;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("substr")]
	internal class SubstrRule : Rule
	{
		private readonly Rule _input;
		private readonly Rule _start;
		private readonly Rule? _count;

		public SubstrRule(Rule input, Rule start)
		{
			_input = input;
			_start = start;
		}
		public SubstrRule(Rule input, Rule start, Rule count)
		{
			_input = input;
			_start = start;
			_count = count;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var input = _input.Apply(data);
			var start = _start.Apply(data);
			var count = _count?.Apply(data);

			if (input.ValueKind != JsonValueKind.String)
				throw new JsonLogicException($"Cannot substring a {input.ValueKind}.");

			var stringInput = input.GetString();
			
			if (start.ValueKind != JsonValueKind.Number)
				throw new JsonLogicException("Start value must be an integer");

			var numberStart = start.GetDecimal();
			if (numberStart != Math.Floor(numberStart))
				throw new JsonLogicException("Start value must be an integer");

			var intStart = (int) Math.Floor(numberStart);
			if (intStart < -stringInput.Length) return input;
			if (intStart < 0)
				intStart = Math.Max(stringInput.Length + intStart, 0);
			if (intStart >= stringInput.Length) return string.Empty.AsJsonElement();

			if (count == null)
				return stringInput[intStart..].AsJsonElement();

			var numberCount = count.Value.GetDecimal();
			if (numberCount != Math.Floor(numberCount))
				throw new JsonLogicException("Count value must be an integer");

			var availableLength = stringInput.Length - intStart;
			var intCount = (int) Math.Floor(numberCount);
			if (intCount < 0)
				intCount = Math.Max(availableLength + intCount, 0);
			intCount = Math.Min(availableLength, intCount);

			return stringInput.Substring(intStart, intCount).AsJsonElement();
		}
	}
}