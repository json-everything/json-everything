using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `var` operation.
/// </summary>
[Operator("var")]
[JsonConverter(typeof(VariableRuleJsonConverter))]
public class VariableRule : Rule
{
	private readonly Rule? _path;
	private readonly Rule? _defaultValue;

	internal VariableRule()
	{
	}
	internal VariableRule(Rule path)
	{
		_path = path;
	}
	internal VariableRule(Rule path, Rule defaultValue)
	{
		_path = path;
		_defaultValue = defaultValue;
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
		if (_path == null) return data;

		var path = _path.Apply(data, contextData);
		var pathString = path.Stringify()!;
		if (pathString == string.Empty) return contextData ?? data;

		var pointer = JsonPointer.Parse(pathString == string.Empty ? "" : $"/{pathString.Replace('.', '/')}");
		if (pointer.TryEvaluate(contextData ?? data, out var pathEval) ||
			pointer.TryEvaluate(data, out pathEval))
			return pathEval;

		return _defaultValue?.Apply(data, contextData) ?? null;
	}
}

internal class VariableRuleJsonConverter : JsonConverter<VariableRule>
{
	public override VariableRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode?>(ref reader, options);

		var parameters = node is JsonArray
			? node.Deserialize<Rule[]>()
			: new[] { node.Deserialize<Rule>()! };

		if (parameters is not ({ Length: 0 } or { Length: 1 } or { Length: 2 }))
			throw new JsonException("The var rule needs an array with 0, 1, or 2 parameters.");

		return parameters.Length switch
		{
			0 => new VariableRule(),
			1 => new VariableRule(parameters[0]),
			_ => new VariableRule(parameters[0], parameters[1])
		};
	}

	public override void Write(Utf8JsonWriter writer, VariableRule value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
