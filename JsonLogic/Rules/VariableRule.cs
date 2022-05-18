using System.Text.Json;
using Json.More;
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

	public override JsonElement Apply(JsonElement data, JsonElement? contextData = null)
	{
		if (_path == null) return data;

		var path = _path.Apply(data, contextData);
		var pathString = path.Stringify()!;
		if (pathString == string.Empty) return contextData ?? data;

		var pointer = JsonPointer.Parse(pathString == string.Empty ? "" : $"/{pathString.Replace('.', '/')}");
		var pathEval = pointer.Evaluate(contextData ?? data) ?? pointer.Evaluate(data);
		if (pathEval != null) return pathEval.Value;

		return _defaultValue?.Apply(data, contextData) ?? ((string?)null).AsJsonElement();
	}
}