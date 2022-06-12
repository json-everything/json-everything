using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("!")]
internal class NotRule : Rule
{
	private readonly Rule _value;

	public NotRule(Rule value)
	{
		_value = value;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var value = _value.Apply(data, contextData);

		return !value.IsTruthy();
	}
}