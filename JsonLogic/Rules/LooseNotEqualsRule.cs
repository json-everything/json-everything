using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("!=")]
internal class LooseNotEqualsRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;

	public LooseNotEqualsRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var a = _a.Apply(data, contextData);
		var b = _b.Apply(data, contextData);

		return !a.LooseEquals(b);
	}
}