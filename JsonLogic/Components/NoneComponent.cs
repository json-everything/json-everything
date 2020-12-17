using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class NoneComponent : ILogicComponent
	{
		private readonly ILogicComponent _input;
		private readonly ILogicComponent _rule;

		public NoneComponent(ILogicComponent input, ILogicComponent rule)
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
			return (!inputData.Select(value => _rule.Apply(value))
					.Any(result => result.IsTruthy()))
				.AsJsonElement();
		}
	}
}