using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("min")]
	internal class MinComponent : LogicComponent
	{
		private readonly List<LogicComponent> _items;

		public MinComponent(LogicComponent a, params LogicComponent[] more)
		{
			_items = new List<LogicComponent> { a };
			_items.AddRange(more);
		}

		public override JsonElement Apply(JsonElement data)
		{
			var items = _items.Select(i => i.Apply(data)).Select(e => new { e.ValueKind, Value = e.Numberify() }).ToList();
			var nulls = items.Where(i => i.Value == null);
			if (nulls.Any())
				throw new JsonLogicException($"Cannot find min with {nulls.First().ValueKind}.");

			return items.Min(i => i.Value.Value).AsJsonElement();
		}
	}
}