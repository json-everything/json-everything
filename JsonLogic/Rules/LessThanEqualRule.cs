using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
#pragma warning disable CS1570

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `<=` operation.
/// </summary>
[Operator("<=")]
[JsonConverter(typeof(LessThanEqualRuleJsonConverter))]
public class LessThanEqualRule : Rule
{
	/// <summary>
	/// The Lower bound.
	/// </summary>
	protected internal Rule A { get; }
	/// <summary>
	/// The middle argument.
	/// </summary>
	protected internal Rule B { get; }
	/// <summary>
	/// The Higher bound.
	/// </summary>
	protected internal Rule? C { get; }

	/// <summary>
	/// Creates a new instance of <see cref="LessThanEqualRule"/> when '<=' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The value to test.</param>
	/// <param name="b">The boundary to test against.</param>
	protected internal LessThanEqualRule(Rule a, Rule b)
	{
		A = a;
		B = b;
	}
	/// <summary>
	/// Creates a new instance of <see cref="LessThanEqualRule"/> when '<=' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The lower bound.</param>
	/// <param name="b">The middle argument.</param>
	/// <param name="c">The upper bound.</param>
	protected internal LessThanEqualRule(Rule a, Rule b, Rule c)
	{
		A = a;
		B = b;
		C = c;
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
		if (C == null)
		{
			var a = A.Apply(data, contextData);
			var b = B.Apply(data, contextData);

			var numberA = a.Numberify();
			var numberB = b.Numberify();

			if (numberA == null || numberB == null)
				throw new JsonLogicException($"Cannot compare {a.JsonType()} and {b.JsonType()}.");

			return numberA <= numberB;
		}

		var low = A.Apply(data, contextData).Numberify();
		if (low == null)
			throw new JsonLogicException("Lower bound must parse to a number.");

		var value = B.Apply(data, contextData).Numberify();
		if (value == null)
			throw new JsonLogicException("Value must parse to a number.");

		var high = C.Apply(data, contextData).Numberify();
		if (high == null)
			throw new JsonLogicException("Upper bound must parse to a number.");

		return low <= value && value <= high;
	}
}

internal class LessThanEqualRuleJsonConverter : JsonConverter<LessThanEqualRule>
{
	public override LessThanEqualRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not ({ Length: 2 } or { Length: 3 }))
			throw new JsonException("The <= rule needs an array with either 2 or 3 parameters.");

		if (parameters.Length == 2) return new LessThanEqualRule(parameters[0], parameters[1]);

		return new LessThanEqualRule(parameters[0], parameters[1], parameters[2]);
	}

	public override void Write(Utf8JsonWriter writer, LessThanEqualRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("<=");
		writer.WriteStartArray();
		writer.WriteRule(value.A, options);
		writer.WriteRule(value.B, options);
		if (value.C != null)
			writer.WriteRule(value.C, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
