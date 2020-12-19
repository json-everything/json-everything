using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("-")]
	internal class SubtractComponent : LogicComponent
	{
		private readonly List<LogicComponent> _items;

		public SubtractComponent(LogicComponent a, params LogicComponent[] more)
		{
			_items = new List<LogicComponent> { a };
			_items.AddRange(more);
		}

		public override JsonElement Apply(JsonElement data)
		{
			if (_items.Count == 0) return 0.AsJsonElement();

			var value = _items[0].Apply(data);
			var number = value.Numberify();

			if (number == null)
				throw new JsonLogicException($"Cannot subtract {value.ValueKind}.");

			var result = number.Value;

			if (_items.Count == 1)
				return (-result).AsJsonElement();
			
			foreach (var item in _items.Skip(1))
			{
				value = item.Apply(data);

				number = value.Numberify();

				if (number == null)
					throw new JsonLogicException($"Cannot subtract {value.ValueKind}.");

				result -= number.Value;
			}

			return result.AsJsonElement();
		}
	}
}