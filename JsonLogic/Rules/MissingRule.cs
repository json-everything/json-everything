using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `missing` operation.
/// </summary>
[Operator("missing")]
[JsonConverter(typeof(MissingRuleJsonConverter))]
public class MissingRule : Rule
{
	private readonly Rule[] _components;

	internal MissingRule(params Rule[] components)
	{
		_components = components;
	}

	/// <summary>
	/// Applies the rule to the input data.
	/// </summary>
	/// <param name="data">The input data.</param>
	/// <param name="contextData">
	///     Optional secondary data.  Used by a few operators to pass a secondary
	///     data context to inner operators.
	/// </param>
	/// <returns>The result of the rule.</returns>
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

internal class MissingRuleJsonConverter : JsonConverter<MissingRule>
{
	public override MissingRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode?>(ref reader, options);

		var parameters = node is JsonArray
			? node.Deserialize<Rule[]>()
			: new[] { node.Deserialize<Rule>()! };

		if (parameters == null) return new MissingRule();

		return new MissingRule(parameters);
	}

	public override void Write(Utf8JsonWriter writer, MissingRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
