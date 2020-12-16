using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class AllComponent : ILogicComponent
	{
		private readonly ILogicComponent _input;
		private readonly ILogicComponent _rule;

		public AllComponent(ILogicComponent input, ILogicComponent rule)
		{
			_input = input;
			_rule = rule;
		}

		public JsonElement Apply(JsonElement data)
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