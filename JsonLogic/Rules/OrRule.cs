using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `or` operation.
/// </summary>
[Operator("or")]
[JsonConverter(typeof(OrRuleJsonConverter))]
public class OrRule : Rule
{
	/// <summary>
	/// The sequence of items to Or against.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="OrRule"/> when 'or' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first value.</param>
	/// <param name="more">Sequence of values to Or against.</param>
	protected internal OrRule(Rule a, params Rule[] more)
	{
		Items = [a, .. more];
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
		var items = Items.Select(i => i.Apply(data, contextData));
		JsonNode? first = false;
		foreach (var x in items)
		{
			first = x;
			if (x.IsTruthy()) break;
		}

		return first;
	}
}

internal class OrRuleJsonConverter : AotCompatibleJsonConverter<OrRule>
{
	public override OrRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.Read(ref reader, JsonLogicSerializerContext.Default.RuleArray);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The + rule needs an array of parameters.");

		return new OrRule(parameters[0], parameters.Skip(1).ToArray());
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override void Write(Utf8JsonWriter writer, OrRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("or");
		writer.WriteRules(value.Items, options);
		writer.WriteEndObject();
	}
}
