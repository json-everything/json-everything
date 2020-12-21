using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("or")]
	internal class OrComponent : LogicComponent
	{
		private readonly List<LogicComponent> _items;

		public OrComponent(LogicComponent a, params LogicComponent[] more)
		{
			_items = new List<LogicComponent> { a };
			_items.AddRange(more);
		}

		public override JsonElement Apply(JsonElement data)
		{
			var items = _items.Select(i => i.Apply(data));
			var first = false.AsJsonElement();
			foreach (var x in items)
			{
				first = x;
				if (x.IsTruthy()) break;
			}
			
			return first;
		}
	}
}