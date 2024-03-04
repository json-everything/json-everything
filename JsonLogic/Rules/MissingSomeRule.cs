using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `missing_some` operation.
/// </summary>
[Operator("missing_some")]
[JsonConverter(typeof(MissingSomeRuleJsonConverter))]
public class MissingSomeRule : Rule
{
	/// <summary>
	/// The minimum number of keys that are required.
	/// </summary>
	protected internal Rule RequiredCount { get; }
	
	/// <summary>
	/// The sequence of keys to search for.
	/// </summary>
	protected internal Rule Components { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MissingSomeRule"/> when 'missing_some' operator is detected within json logic.
	/// </summary>
	/// <param name="requiredCount">The minimum number of data keys that are required.</param>
	/// <param name="components">A sequence of keys to search for.</param>
	protected internal MissingSomeRule(Rule requiredCount, Rule components)
	{
		RequiredCount = requiredCount;
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
		var requiredCount = RequiredCount.Apply(data, contextData).Numberify() ?? 1;
		var components = Components.Apply(data, contextData);
		if (components is not JsonArray arr)
			arr = new JsonArray(components?.DeepClone());

		var expected = arr.SelectMany(e => e.Flatten()).ToList();

		if (data is not JsonObject)
			return expected.ToJsonArray();

		var paths = expected
			.Select(p =>
			{
				if (p is JsonValue v && v.TryGetValue(out string? s))
					return new { Path = p, Pointer = JsonPointer.Parse(s == string.Empty ? "" : $"/{s.Replace('.', '/')}") };
				return new { Path = p, Pointer = (JsonPointer?)null }!;
			})
			.Select(p =>
			{
				if (p.Pointer != null! && p.Pointer.TryEvaluate(data, out var value))
					return new { Path = p.Path, Value = value };
				return new { Path = p.Path, Value = (JsonNode?)null };
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

internal class MissingSomeRuleJsonConverter : WeaklyTypedJsonConverter<MissingSomeRule>
{
	public override MissingSomeRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not { Length: 2 })
			throw new JsonException("The missing_some rule needs an array with 2 parameters.");

		return new MissingSomeRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, MissingSomeRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("missing_some");
		writer.WriteStartArray();
		options.Write(writer, value.RequiredCount, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.Components, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
