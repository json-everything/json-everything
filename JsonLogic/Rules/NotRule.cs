using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("!")]
	internal class NotRule : Rule
	{
		private readonly Rule _value;

		public NotRule(Rule value)
		{
			_value = value;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var value = _value.Apply(data);

			return (!value.IsTruthy()).AsJsonElement();
		}
	}
}