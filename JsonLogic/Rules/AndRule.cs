using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("and")]
	internal class AndRule : Rule
	{
		private readonly List<Rule> _items;

		public AndRule(Rule a, params Rule[] more)
		{
			_items = new List<Rule> { a };
			_items.AddRange(more);
		}

		public override JsonElement Apply(JsonElement data)
		{
			var items = _items.Select(i => i.Apply(data));
			var first = false.AsJsonElement();
			foreach (var x in items)
			{
				first = x;
				if (!x.IsTruthy()) break;
			}

			return first;
		}
	}
}