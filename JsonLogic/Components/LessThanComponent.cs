using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("<")]
	internal class LessThanComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public LessThanComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			if (a.ValueKind != JsonValueKind.Number || b.ValueKind != JsonValueKind.Number)
				throw new JsonLogicException($"Cannot compare {a.ValueKind} and {b.ValueKind}.");

			return (a.GetDecimal() < b.GetDecimal()).AsJsonElement();
		}
	}
}