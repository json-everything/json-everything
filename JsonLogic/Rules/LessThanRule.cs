using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
#pragma warning disable CS1570

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `<` operation.
/// </summary>
[Operator("<")]
[JsonConverter(typeof(LessThanRuleJsonConverter))]
public class LessThanRule : Rule
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
	/// Creates a new instance of <see cref="LessThanRule"/> when '<' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The argument to test.</param>
	/// <param name="b">The boundary to test against.</param>
	protected internal LessThanRule(Rule a, Rule b)
	{
		A = a;
		B = b;
	}

	/// <summary>
	/// Creates a new instance of <see cref="LessThanRule"/> when '<' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The lower bound.</param>
	/// <param name="b">The middle argument.</param>
	/// <param name="c">The upper bound.</param>
	protected internal LessThanRule(Rule a, Rule b, Rule c)
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
		var a = A.Apply(data, contextData);
		var b = B.Apply(data, contextData);

		var av = a as JsonValue;
		var bv = b as JsonValue;

		string? stringA, stringB;

		if (C == null)
		{
			if (av != null && av.TryGetValue(out stringA) &&
			    bv != null && bv.TryGetValue(out stringB))
				return string.Compare(stringA, stringB, StringComparison.Ordinal) < 0;

			var numberA = a.Numberify();
			var numberB = b.Numberify();

			if (numberA != null && numberB != null) return numberA < numberB;
			if (numberA != null || numberB != null) return false;

			stringA = a.Stringify();
			stringB = b.Stringify();

			return string.Compare(stringA, stringB, StringComparison.Ordinal) < 0;
		}

		var c = C.Apply(data, contextData);
		var cv = c as JsonValue;

		if (av != null && av.TryGetValue(out stringA) &&
		    bv != null && bv.TryGetValue(out stringB) &&
		    cv != null && cv.TryGetValue(out string? stringC))
			return string.Compare(stringA, stringB, StringComparison.Ordinal) < 0 &&
			       string.Compare(stringB, stringC, StringComparison.Ordinal) < 0;

		var low = a.Numberify();
		var value = b.Numberify();
		var high = c.Numberify();
		if (low != null && value != null && high != null) return low < value && value < high;
		if (low != null || value != null || high != null) return false;

		stringA = a.Stringify();
		stringB = b.Stringify();
		stringC = c.Stringify();

		return string.Compare(stringA, stringB, StringComparison.Ordinal) < 0 &&
		       string.Compare(stringB, stringC, StringComparison.Ordinal) < 0;
	}
}

internal class LessThanRuleJsonConverter : WeaklyTypedJsonConverter<LessThanRule>
{
	public override LessThanRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not ({ Length: 2 } or { Length: 3 }))
			throw new JsonException("The < rule needs an array with either 2 or 3 parameters.");

		if (parameters.Length == 2) return new LessThanRule(parameters[0], parameters[1]);

		return new LessThanRule(parameters[0], parameters[1], parameters[2]);
	}

	public override void Write(Utf8JsonWriter writer, LessThanRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("<");
		writer.WriteStartArray();
		options.Write(writer, value.A, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.B, JsonLogicSerializerContext.Default.Rule);
		if (value.C != null)
			options.Write(writer, value.C, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
