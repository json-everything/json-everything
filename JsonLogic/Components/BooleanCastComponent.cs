using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class BooleanCastComponent : ILogicComponent
	{
		private readonly ILogicComponent _value;

		public BooleanCastComponent(ILogicComponent value)
		{
			_value = value;
		}
		
		public JsonElement Apply(JsonElement data)
		{
			var value = _value.Apply(data);

			if (value.ValueKind == JsonValueKind.Object)
				throw new JsonLogicException("Cannot cast objects to boolean");
			
			return _value.Apply(data).IsTruthy().AsJsonElement();
		}
	}
}