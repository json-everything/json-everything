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
	/// <summary>
	/// A sequence of keys to search for.
	/// </summary>
	protected internal Rule[] Components { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MissingRule"/> when 'missing' operator is detected within json logic.
	/// </summary>
	/// <param name="components">A sequence of keys to search for.</param>
	protected internal MissingRule(params Rule[] components)
	{
		Components = components;
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
		var expected = Components.SelectMany(c => c.Apply(data, contextData).Flatten())
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

		return paths.Where(p => p.Value == null || p.Value.IsEquivalentTo(string.Empty))
			.Select(k => (JsonNode?)k.Path)
			.ToJsonArray();

	}
}

internal class MissingRuleJsonConverter : WeaklyTypedJsonConverter<MissingRule>
{
	public override MissingRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule)
			: new[] { options.Read(ref reader, JsonLogicSerializerContext.Default.Rule)! };

		if (parameters == null) return new MissingRule();

		return new MissingRule(parameters);
	}

	public override void Write(Utf8JsonWriter writer, MissingRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("missing");
		options.WriteList(writer, value.Components, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}