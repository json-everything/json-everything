using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class AddComponent : ILogicComponent
	{
		private readonly ILogicComponent _a;
		private readonly ILogicComponent _b;

		public AddComponent(ILogicComponent a)
		{
			_a = a;
		}
		public AddComponent(ILogicComponent a, ILogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public JsonElement Apply(JsonElement data)
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