using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class BooleanCastComponent : LogicComponent
	{
		private readonly LogicComponent _value;

		public BooleanCastComponent(LogicComponent value)
		{
			_value = value;
		}
		
		public override JsonElement Apply(JsonElement data)
		{
			var value = _value.Apply(data);

			if (value.ValueKind == JsonValueKind.Object)
				throw new JsonLogicException("Cannot cast objects to boolean");
			
			return _value.Apply(data).IsTruthy().AsJsonElement();
		}
	}
}