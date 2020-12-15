using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Path;

namespace Json.Logic.Components
{
	internal class VariableComponent : ILogicComponent
	{
		private readonly JsonPath _path;
		private readonly ILogicComponent _defaultValue;

		public VariableComponent(JsonPath path, ILogicComponent defaultValue = null)
		{
			_path = path;
			_defaultValue = defaultValue;
		}

		public JsonElement Apply(JsonElement data)
		{
			var pathEval = _path.Evaluate(data).Matches;
			if (pathEval != null)
			{
				if (pathEval.Count == 1) return pathEval[0].Value;

				return pathEval.Select(m => m.Value).AsJsonElement();
			}

			return _defaultValue?.Apply(data) ??
			       throw new JsonLogicException($"Could not find value at `{_path}` in provided data and no default was provided.");
		}
	}
}