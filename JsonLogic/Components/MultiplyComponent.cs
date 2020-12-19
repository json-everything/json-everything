using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("*")]
	internal class MultiplyComponent : LogicComponent
	{
		private readonly List<LogicComponent> _items;

		public MultiplyComponent(LogicComponent a, params LogicComponent[] more)
		{
			_items = new List<LogicComponent> {a};
			_items.AddRange(more);
		}

		public override JsonElement Apply(JsonElement data)
		{
			decimal result = 1;

			foreach (var item in _items)
			{
				var value = item.Apply(data);

				var number = value.Numberify();
				
				if (number == null)
					throw new JsonLogicException($"Cannot multiply {value.ValueKind}.");

				result *= number.Value;
			}

			return result.AsJsonElement();
		}
	}
}