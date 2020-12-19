using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class AddComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public AddComponent(LogicComponent a)
		{
			_a = a;
		}
		public AddComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);

			if (_b != null)
			{
				var b = _b.Apply(data);

				if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
					return (a.GetDecimal() + b.GetDecimal()).AsJsonElement();

				throw new JsonLogicException($"Cannot add types {a.ValueKind} and {b.ValueKind}.");
			}

			return a.ValueKind switch
			{
				JsonValueKind.String => decimal.TryParse(a.GetString(), out var d)
					? d.AsJsonElement()
					: throw new JsonLogicException($"Cannot cast {a.ToJsonString()} to number."),
				JsonValueKind.Number => a,
				_ => throw new JsonLogicException($"Cannot cast {a.ValueKind} to number.")
			};
		}
	}
}