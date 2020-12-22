using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("filter")]
	internal class FilterComponent : LogicComponent
	{
		private readonly LogicComponent _input;
		private readonly LogicComponent _rule;

		public FilterComponent(LogicComponent input, LogicComponent rule)
		{
			_input = input;
			_rule = rule;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var input = _input.Apply(data);

			if (input.ValueKind != JsonValueKind.Array)
				return new JsonElement[0].AsJsonElement();

			return input.EnumerateArray().Where(i => _rule.Apply(i).IsTruthy()).AsJsonElement();
		}
	}
}