using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `merge` operation.
/// </summary>
[Operator("merge")]
[JsonConverter(typeof(MergeRuleJsonConverter))]
public class MergeRule : Rule
{
	private readonly List<Rule> _items;

	internal MergeRule(params Rule[] items)
	{
		_items = items.ToList();
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
		var items = _items.Select(i => i.Apply(data, contextData)).SelectMany(e => e.Flatten());

		return items.ToJsonArray();
	}
}

internal class MergeRuleJsonConverter : JsonConverter<MergeRule>
{
	public override MergeRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters == null) return new MergeRule();

		return new MergeRule(parameters);
	}

	public override void Write(Utf8JsonWriter writer, MergeRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
