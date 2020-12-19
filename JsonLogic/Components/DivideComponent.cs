using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("/")]
	internal class DivideComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public DivideComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			var numberA = a.Numberify();
			var numberB = b.Numberify();

			if (numberA == null || numberB == null)
				throw new JsonLogicException($"Cannot divide types {a.ValueKind} and {b.ValueKind}.");

			if (numberB == 0)
				throw new JsonLogicException("Cannot divide by zero");

			return (numberA.Value / numberB.Value).AsJsonElement();
		}
	}
}