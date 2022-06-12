using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
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
			return expected.ToJsonArray();

		var paths = expected.Select(e => e.GetValue<string?>()!)
			.Select(p => new { Path = p, Pointer = JsonPointer.Parse(p == string.Empty ? "" : $"/{p.Replace('.', '/')}") })
			.Select(p =>
			{
				p.Pointer.TryEvaluate(data, out var value);
				return new { Path = p.Path, Value = value };
			});

		return paths.Where(p => p.Value == null)
			.Select(k => (JsonNode?)k.Path)
			.ToJsonArray();

	}
}