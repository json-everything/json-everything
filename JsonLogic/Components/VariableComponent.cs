using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Logic.Components
{
	[Operator("var")]
	internal class VariableComponent : LogicComponent
	{
		private readonly LogicComponent _path;
		private readonly LogicComponent _defaultValue;

		public VariableComponent()
		{
		}
		public VariableComponent(LogicComponent path)
		{
			_path = path;
		}
		public VariableComponent(LogicComponent path, LogicComponent defaultValue)
		{
			_path = path;
			_defaultValue = defaultValue;
		}

		public override JsonElement Apply(JsonElement data)
		{
			if (_path == null) return data;
			
			var path = _path.Apply(data);
			var pathString = path.Stringify();
			var pointer = JsonPointer.Parse(pathString == string.Empty ? "" : $"/{pathString.Replace('.', '/')}");
			var pathEval = pointer.Evaluate(data);
			if (pathEval != null) return pathEval.Value;

			return _defaultValue?.Apply(data) ?? ((string) null).AsJsonElement();
		}
	}
}