using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("filter")]
	internal class FilterRule : Rule
	{
		private readonly Rule _input;
		private readonly Rule _rule;

		public FilterRule(Rule input, Rule rule)
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