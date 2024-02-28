using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `if` and `?:` operations.
/// </summary>
[Operator("if")]
[Operator("?:")]
[JsonConverter(typeof(IfRuleJsonConverter))]
public class IfRule : Rule
{
	/// <summary>
	/// A condition, what to do when the condition is true, and what to do when the condition is false. 
	/// </summary>
	protected internal List<Rule> Components { get; }

	/// <summary>
	/// Creates a new instance of <see cref="IfRule"/> when 'if' or '?:' operators are detected within json logic.
	/// </summary>
	/// <param name="components">A condition, what to do when the condition is true, and what to do when the condition is false.</param>
	protected internal IfRule(params Rule[] components)
	{
		Components = new List<Rule>(components);
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
		bool condition;
		switch (Components.Count)
		{
			case 0:
				return null;
			case 1:
				return Components[0].Apply(data, contextData);
			case 2:
				condition = Components[0].Apply(data, contextData).IsTruthy();
				var thenResult = Components[1];

				return condition
					? thenResult.Apply(data, contextData)
					: null;
			default:
				var currentCondition = Components[0];
				var currentTrueResult = Components[1];
				var elseIndex = 2;

				while (currentCondition != null)
				{
					condition = currentCondition.Apply(data, contextData).IsTruthy();

					if (condition)
						return currentTrueResult.Apply(data, contextData);

					if (elseIndex == Components.Count) return null;

					currentCondition = Components[elseIndex++];

					if (elseIndex >= Components.Count)
						return currentCondition.Apply(data, contextData);

					currentTrueResult = Components[elseIndex++];
				}
				break;
		}

		throw new NotImplementedException("Something went wrong. This shouldn't happen.");
	}
}

internal class IfRuleJsonConverter : WeaklyTypedJsonConverter<IfRule>
{
	public override IfRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters == null) return new IfRule();

		return new IfRule(parameters);
	}

	public override void Write(Utf8JsonWriter writer, IfRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("if");
		options.WriteList(writer, value.Components, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
