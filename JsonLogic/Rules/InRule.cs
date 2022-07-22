using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `in` operation.
/// </summary>
[Operator("in")]
[JsonConverter(typeof(InRuleJsonConverter))]
public class InRule : Rule
{
	private readonly Rule _test;
	private readonly Rule _source;

	internal InRule(Rule test, Rule source)
	{
		_test = test;
		_source = source;
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
		var test = _test.Apply(data, contextData);
		var source = _source.Apply(data, contextData);

		if (source is JsonValue value && value.TryGetValue(out string? stringSource))
		{
			var stringTest = test.Stringify();

			if (stringTest == null || stringSource == null)
				throw new JsonLogicException($"Cannot check string for {test.JsonType()}.");

			return !string.IsNullOrEmpty(stringTest) && stringSource.Contains(stringTest);
		}

		if (source is JsonArray arr)
			return arr.Any(i => i.IsEquivalentTo(test));

		return false;
	}
}

internal class InRuleJsonConverter : JsonConverter<InRule>
{
	public override InRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = JsonSerializer.Deserialize<Rule[]>(ref reader, options);

		if (parameters is not { Length: 2 })
			throw new JsonException("The in rule needs an array with 2 parameters.");

		return new InRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, InRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
