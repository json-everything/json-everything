using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class ModComponent : ILogicComponent
	{
		private readonly ILogicComponent _a;
		private readonly ILogicComponent _b;

		public ModComponent(ILogicComponent a, ILogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public JsonElement Apply(JsonElement data)
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