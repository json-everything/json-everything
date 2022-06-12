using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Logic.Rules;

[Operator("missing")]
internal class MissingRule : Rule
{
	private readonly Rule[] _components;

	public MissingRule(params Rule[] components)
	{
		_components = components;
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var expected = _components.SelectMany(c => c.Apply(data, contextData).Flatten())
			.OfType<JsonValue>()
			.Where(v => v.TryGetValue(out string? _));

		if (data is not JsonObject)
			return new JsonArray(expected.Cast<JsonNode>().ToArray());

		var paths = expected.Select(e => e.GetValue<string?>()!)
			.Select(p => new { Path = p, Pointer = JsonPointer.Parse(p == string.Empty ? "" : $"/{p.Replace('.', '/')}") })
			.Select(p =>
			{
				p.Pointer.TryEvaluate(data, out var value);
				return new { Path = p.Path, Value = value };
			});

		return new JsonArray(paths.Where(p => p.Value == null)
			.Select(k => (JsonNode?)k.Path)
			.ToArray());

	}
}