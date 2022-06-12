using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

[Operator("<")]
internal class LessThanRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;
	private readonly Rule? _c;

	public LessThanRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
	}

	public LessThanRule(Rule a, Rule b, Rule c)
	{
		_a = a;
		_b = b;
		_c = c;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		if (_c == null)
		{
			var a = _a.Apply(data, contextData);
			var b = _b.Apply(data, contextData);

			var numberA = a.Numberify();
			var numberB = b.Numberify();

			if (numberA == null || numberB == null)
				throw new JsonLogicException($"Cannot compare {a.JsonType()} and {b.JsonType()}.");

			return numberA < numberB;
		}

		var low = (_a.Apply(data, contextData) as JsonValue)?.GetNumber();
		if (low == null)
			throw new JsonLogicException("Lower bound must be a number.");

		var value = (_b.Apply(data, contextData) as JsonValue)?.GetNumber();
		if (value == null)
			throw new JsonLogicException("Value must be a number.");

		var high = (_c.Apply(data, contextData) as JsonValue)?.GetNumber();
		if (high == null)
			throw new JsonLogicException("Upper bound must be a number.");

		return low < value && value < high;
	}
}