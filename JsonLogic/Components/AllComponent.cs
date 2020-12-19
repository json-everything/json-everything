using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("all")]
	internal class AllComponent : LogicComponent
	{
		private readonly LogicComponent _input;
		private readonly LogicComponent _rule;

		public AllComponent(LogicComponent input, LogicComponent rule)
		{
			_input = input;
			_rule = rule;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var input = _input.Apply(data);

			if (input.ValueKind != JsonValueKind.Array)
				throw new JsonLogicException("Input must evaluate to an array.");

			var inputData = input.EnumerateArray();
			return inputData.Select(value => _rule.Apply(value))
				.All(result => result.IsTruthy())
				.AsJsonElement();
		}
	}
}