using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("/")]
internal class DivideRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;

	public DivideRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
	}

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

		return numberA.Value / numberB.Value;
	}
}