using System.Linq;
using System.Text.Json;
using System.Threading;
using Json.More;
using Json.Path;

namespace Json.Logic.Components
{
	[Operator("var")]
	internal class VariableComponent : LogicComponent
	{
		private readonly LogicComponent _path;
		private readonly LogicComponent _defaultValue;

		public VariableComponent(LogicComponent path, LogicComponent defaultValue = null)
		{
			_path = path;
			_defaultValue = defaultValue;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var path = _path.Apply(data);
			if (path.ValueKind != JsonValueKind.String)
				throw new JsonLogicException("Path must be a string.");

			var pathString = path.GetString();
			var jsonPath = JsonPath.Parse(pathString == string.Empty ? "$" : $"$.{pathString}");
			var pathEval = jsonPath.Evaluate(data).Matches;
			if (pathEval != null && pathEval.Count != 0)
			{
				if (pathEval.Count == 1) return pathEval[0].Value;

				return pathEval.Select(m => m.Value).AsJsonElement();
			}

			return _defaultValue?.Apply(data) ??
			       throw new JsonLogicException($"Could not find value at `{_path}` in provided data and no default was provided.");
		}
	}
}