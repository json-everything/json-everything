using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Logic.Rules
{
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

		public override JsonElement Apply(JsonElement data)
		{
			if (_path == null) return data;
			
			var path = _path.Apply(data);
			var pathString = path.Stringify()!;
			var pointer = JsonPointer.Parse(pathString == string.Empty ? "" : $"/{pathString.Replace('.', '/')}");
			var pathEval = pointer.Evaluate(data);
			if (pathEval != null) return pathEval.Value;

			return _defaultValue?.Apply(data) ?? ((string?) null).AsJsonElement();
		}
	}
}