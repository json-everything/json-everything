using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class SubtractComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public SubtractComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
				return (a.GetDecimal() - b.GetDecimal()).AsJsonElement();

			throw new JsonLogicException($"Cannot subtract types {a.ValueKind} and {b.ValueKind}.");
		}
	}
}