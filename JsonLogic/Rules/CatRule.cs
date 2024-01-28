using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `cat` operation.
/// </summary>
[Operator("cat")]
[JsonConverter(typeof(CatRuleJsonConverter))]
public class CatRule : Rule
{
	/// <summary>
	/// The sequence of values to concatenate together.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="CatRule"/> when 'cat' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value, to which subsequent values will be concatenated to.</param>
	/// <param name="more">A sequence of values to concatenate to the first value.</param>
	protected internal CatRule(Rule a, params Rule[] more)
	{
		Items = new List<Rule> { a };
		Items.AddRange(more);
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
		var result = string.Empty;

		foreach (var item in Items)
		{
			var value = item.Apply(data, contextData);

			var str = value.Stringify();

			result += str ?? throw new JsonLogicException($"Cannot concatenate {value.JsonType()}.");
		}

		return result;
	}
}

internal class CatRuleJsonConverter : AotCompatibleJsonConverter<CatRule>
{
	public override CatRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = options.Read(ref reader, LogicSerializerContext.Default.JsonNode);

		var parameters = node is JsonArray
			? node.Deserialize(LogicSerializerContext.Default.RuleArray)
			: new[] { node.Deserialize(LogicSerializerContext.Default.Rule)! };

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The cat rule needs an array of parameters.");

		return new CatRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, CatRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("cat");
		writer.WriteRules(value.Items, options);
		writer.WriteEndObject();
	}
}
