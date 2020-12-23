using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("all")]
	internal class AllRule : Rule
	{
		private readonly Rule _input;
		private readonly Rule _rule;

		public AllRule(Rule input, Rule rule)
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
			var results = inputData.Select(value => _rule.Apply(value)).ToList();
			return (results.Any() &&
				   results.All(result => result.IsTruthy()))
				.AsJsonElement();
		}
	}
}