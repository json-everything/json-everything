using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic.Rules;

[Operator("in")]
internal class InRule : Rule
{
	private readonly Rule _test;
	private readonly Rule _source;

	public InRule(Rule test, Rule source)
	{
		_test = test;
		_source = source;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var test = _test.Apply(data, contextData);
		var source = _source.Apply(data, contextData);

		if (source is JsonValue value && value.TryGetValue(out string? stringSource))
		{
			var stringTest = test.Stringify();

			if (stringTest == null || stringSource == null)
				throw new JsonLogicException($"Cannot check string for {test.JsonType()}.");

			return !string.IsNullOrEmpty(stringTest) && stringSource.Contains(stringTest);
		}

		if (source is JsonArray arr)
			return arr.Any(i => i.IsEquivalentTo(test));

		return false;
	}
}