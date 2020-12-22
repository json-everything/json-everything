using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("==")]
	internal class LooseEqualsRule : Rule
	{
		private readonly Rule _a;
		private readonly Rule _b;

		public LooseEqualsRule(Rule a, Rule b)
		{
			_a = a;
			_b = b;
		}
	
		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			return a.LooseEquals(b).AsJsonElement();
		}
	}
}