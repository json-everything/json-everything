﻿using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `%` operation.
/// </summary>
[Operator("%")]
public class ModRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;

	internal ModRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
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
		var a = _a.Apply(data, contextData);
		var b = _b.Apply(data, contextData);

		var numberA = a.Numberify();
		var numberB = b.Numberify();

		if (numberA == null || numberB == null)
			throw new JsonLogicException($"Cannot divide types {a.JsonType()} and {b.JsonType()}.");

		if (numberB == 0)
			throw new JsonLogicException("Cannot divide by zero");

		return numberA.Value % numberB.Value;
	}
}