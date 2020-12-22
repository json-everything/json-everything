using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("merge")]
	internal class MergeRule : Rule
	{
		private readonly List<Rule> _items;

		public MergeRule(params Rule[] items)
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