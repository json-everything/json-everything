using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Logic.Rules;

[Operator("var")]
internal class VariableRule : Rule
{
	private readonly Rule? _path;
	private readonly Rule? _defaultValue;

	public VariableRule()
	{
	}
	public VariableRule(Rule path)
	{
		_path = path;
	}
	public VariableRule(Rule path, Rule defaultValue)
	{
		_path = path;
		_defaultValue = defaultValue;
	}

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