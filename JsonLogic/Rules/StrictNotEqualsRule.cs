using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("!==")]
	internal class StrictNotEqualsRule : Rule
	{
		private readonly Rule _a;
		private readonly Rule _b;

		public StrictNotEqualsRule(Rule a, Rule b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			return (!_a.Apply(data).IsEquivalentTo(_b.Apply(data))).AsJsonElement();
		}
	}
}