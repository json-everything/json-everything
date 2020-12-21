using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("cat")]
	internal class CatComponent : LogicComponent
	{
		private readonly List<LogicComponent> _items;

		public CatComponent(LogicComponent a, params LogicComponent[] more)
		{
			_items = new List<LogicComponent> { a };
			_items.AddRange(more);
		}

		public override JsonElement Apply(JsonElement data)
		{
			var result = string.Empty;

			foreach (var item in _items)
			{
				var value = item.Apply(data);

				var str = value.Stringify();

				result += str ?? throw new JsonLogicException($"Cannot concatenate {value.ValueKind}.");
			}

			return result.AsJsonElement();
		}
	}
}