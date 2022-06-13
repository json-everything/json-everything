using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("!!")]
internal class BooleanCastRule : Rule
{
	private readonly Rule _value;

	public BooleanCastRule(Rule value)
	{
		_value = value;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var value = _value.Apply(data, contextData);

		return _value.Apply(data, contextData).IsTruthy();
	}
}