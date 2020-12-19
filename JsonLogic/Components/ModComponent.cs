using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("%")]
	internal class ModComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public ModComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
			{
				var bValue = b.GetDecimal();
				if (bValue == 0)
					throw new JsonLogicException("Cannot divide by zero");

				return (a.GetDecimal() % b.GetDecimal()).AsJsonElement();
			}

			throw new JsonLogicException($"Cannot divide types {a.ValueKind} and {b.ValueKind}.");
		}
	}
}