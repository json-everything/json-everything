using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

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

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var input = _input.Apply(data, contextData);
		var start = _start.Apply(data, contextData);

		if (input is not JsonValue inputValue || !inputValue.TryGetValue(out string? stringInput))
			throw new JsonLogicException($"Cannot substring a {input.JsonType()}.");

		if (start is not JsonValue startValue || inputValue.GetNumber() == null)
			throw new JsonLogicException("Start value must be an integer");

		var numberStart = startValue.GetNumber()!.Value;
		if (numberStart != Math.Floor(numberStart))
			throw new JsonLogicException("Start value must be an integer");

		var intStart = (int)Math.Floor(numberStart);
		if (intStart < -stringInput.Length) return input;
		if (intStart < 0)
			intStart = Math.Max(stringInput.Length + intStart, 0);
		if (intStart >= stringInput.Length) return string.Empty;

		if (_count == null) return stringInput[intStart..];

		var count = _count.Apply(data, contextData);
		if (count is not JsonValue countValue || countValue.GetInteger() == null)
			throw new JsonLogicException("Count value must be an integer");

		var integerCount = (int)countValue.GetInteger()!.Value;
		var availableLength = stringInput.Length - intStart;
		if (integerCount < 0)
			integerCount = Math.Max(availableLength + integerCount, 0);
		integerCount = Math.Min(availableLength, integerCount);

		return stringInput.Substring(intStart, integerCount);
	}
}