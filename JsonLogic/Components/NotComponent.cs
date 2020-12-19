using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class NotComponent : LogicComponent
	{
		private readonly LogicComponent _value;

		public NotComponent(LogicComponent value)
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