using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("merge")]
	internal class MergeComponent : LogicComponent
	{
		private readonly List<LogicComponent> _items;

		public MergeComponent(params LogicComponent[] items)
		{
			_items = items.ToList();
		}
	
		public override JsonElement Apply(JsonElement data)
		{
			var items = _items.Select(i => i.Apply(data)).SelectMany(e => e.Flatten());

			return items.AsJsonElement();
		}
	}
}