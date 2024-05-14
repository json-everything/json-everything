using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Provides a stand-in "rule" for collections of rules.
/// </summary>
/// <remarks>This is not exactly part of the specification, but it helps things in this library.</remarks>
[JsonConverter(typeof(RuleCollectionJsonConverter))]
public class RuleCollection : Rule
{
	internal IEnumerable<Rule> Rules { get; }

	internal RuleCollection(params Rule[] rules)
	{
		Rules = rules;
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
		return Rules.Select(x => x.Apply(data, contextData)).ToJsonArray();
	}
}

internal class RuleCollectionJsonConverter : WeaklyTypedJsonConverter<RuleCollection>
{
	public override RuleCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// not used
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, RuleCollection value, JsonSerializerOptions options)
	{
		options.WriteList(writer, value.Rules, JsonLogicSerializerContext.Default.Rule);
	}
}