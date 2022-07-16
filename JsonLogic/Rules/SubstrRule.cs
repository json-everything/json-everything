using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `substr` operation.
/// </summary>
[Operator("substr")]
public class SubstrRule : Rule
{
	private readonly Rule _input;
	private readonly Rule _start;
	private readonly Rule? _count;

	internal SubstrRule(Rule input, Rule start)
	{
		_input = input;
		_start = start;
	}
	internal SubstrRule(Rule input, Rule start, Rule count)
	{
		_input = input;
		_start = start;
		_count = count;
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
		var input = _input.Apply(data, contextData);
		var start = _start.Apply(data, contextData);

		if (input is not JsonValue inputValue || !inputValue.TryGetValue(out string? stringInput))
			throw new JsonLogicException($"Cannot substring a {input.JsonType()}.");

		if (start is not JsonValue startValue || startValue.GetInteger() == null)
			throw new JsonLogicException("Start value must be an integer");

		var numberStart = (int)startValue.GetInteger()!.Value;

		if (numberStart < -stringInput.Length) return input;
		if (numberStart < 0)
			numberStart = Math.Max(stringInput.Length + numberStart, 0);
		if (numberStart >= stringInput.Length) return string.Empty;

		if (_count == null) return stringInput[numberStart..];

		var count = _count.Apply(data, contextData);
		if (count is not JsonValue countValue || countValue.GetInteger() == null)
			throw new JsonLogicException("Count value must be an integer");

		var integerCount = (int)countValue.GetInteger()!.Value;
		var availableLength = stringInput.Length - numberStart;
		if (integerCount < 0)
			integerCount = Math.Max(availableLength + integerCount, 0);
		integerCount = Math.Min(availableLength, integerCount);

		return stringInput.Substring(numberStart, integerCount);
	}
}