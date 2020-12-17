using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class NotComponent : ILogicComponent
	{
		private readonly ILogicComponent _value;

		public NotComponent(ILogicComponent value)
		{
			_value = value;
		}

		public JsonElement Apply(JsonElement data)
		{
			var value = _value.Apply(data);

			return (!value.IsTruthy()).AsJsonElement();
		}
	}
}