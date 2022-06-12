using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

[Operator("")]
internal class LiteralRule : Rule
{
	private readonly JsonNode? _value;

	public static readonly LiteralRule Null = new(null);

	public LiteralRule(JsonNode? value)
	{
		_value = value.Copy();
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		return _value;
	}
}