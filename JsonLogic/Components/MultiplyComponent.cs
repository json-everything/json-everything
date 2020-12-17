using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class MultiplyComponent : ILogicComponent
	{
		private readonly ILogicComponent _a;
		private readonly ILogicComponent _b;

		public MultiplyComponent(ILogicComponent a, ILogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
				return (a.GetDecimal() * b.GetDecimal()).AsJsonElement();

			throw new JsonLogicException($"Cannot multiply types {a.ValueKind} and {b.ValueKind}.");
		}
	}
}