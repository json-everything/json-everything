using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("+")]
	internal class AddComponent : LogicComponent
	{
		private readonly List<LogicComponent> _items;

		public AddComponent(LogicComponent a, params LogicComponent[] more)
		{
			_items = new List<LogicComponent> { a };
			_items.AddRange(more);
		}

		public override JsonElement Apply(JsonElement data)
		{
			decimal result = 0;

			foreach (var item in _items)
			{
				var value = item.Apply(data);

				var number = value.Numberify();

				if (number == null)
					throw new JsonLogicException($"Cannot add {value.ValueKind}.");

				result += number.Value;
			}

			return result.AsJsonElement();
		}
	}
}