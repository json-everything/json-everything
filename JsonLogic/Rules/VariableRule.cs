using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `var` operation.
/// </summary>
[Operator("var")]
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