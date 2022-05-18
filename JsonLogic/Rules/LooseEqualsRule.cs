using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules;

[Operator("==")]
internal class LooseEqualsRule : Rule
{
	private readonly Rule _a;
	private readonly Rule _b;

	public LooseEqualsRule(Rule a, Rule b)
	{
		_a = a;
		_b = b;
	}

	public override JsonElement Apply(JsonElement data, JsonElement? contextData = null)
	{
		var a = _a.Apply(data, contextData);
		var b = _b.Apply(data, contextData);

		return a.LooseEquals(b).AsJsonElement();
	}
}