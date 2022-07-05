using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;

namespace Json.Logic.Rules;

[Operator("missing_some")]
internal class MissingSomeRule : Rule
{
	private readonly Rule _requiredCount;
	private readonly Rule _components;

	public MissingSomeRule(Rule requiredCount, Rule components)
	{
		_requiredCount = requiredCount;
		_components = components;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var requiredCount = _requiredCount.Apply(data, contextData).Numberify();
		var components = _components.Apply(data, contextData);
		if (components is not JsonArray arr)
			throw new JsonLogicException("Expected array of required paths.");

		var expected = arr.SelectMany(e => e.Flatten()).ToList();
		if (expected.Any(e => e is JsonValue v && !v.TryGetValue(out string? _)))
			throw new JsonLogicException("Expected array of required paths.");

		if (data is not JsonObject)
			return expected.ToJsonArray();

		var paths = expected.Cast<JsonValue>().Select(e => e.GetValue<string?>()!)
			.Select(p => new { Path = p, Pointer = JsonPointer.Parse(p == string.Empty ? "" : $"/{p.Replace('.', '/')}") })
			.Select(p =>
			{
				p.Pointer.TryEvaluate(data, out var value);
				return new { Path = p.Path, Value = value };
			})
			.ToList();

		var missing = paths.Where(p => p.Value == null)
			.Select(k => (JsonNode?)k.Path);
		var found = paths.Count(p => p.Value != null);

		if (found < requiredCount)
			return missing.ToJsonArray();

		return new JsonArray();
	}
}