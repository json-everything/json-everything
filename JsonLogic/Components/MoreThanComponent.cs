using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class MoreThanComponent : ILogicComponent
	{
		private readonly ILogicComponent _a;
		private readonly ILogicComponent _b;

		public MoreThanComponent(ILogicComponent a, ILogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			if (a.ValueKind != JsonValueKind.Number || b.ValueKind != JsonValueKind.Number)
				throw new JsonLogicException($"Cannot compare {a.ValueKind} and {b.ValueKind}.");

			return (a.GetDecimal() > b.GetDecimal()).AsJsonElement();
		}
	}
}