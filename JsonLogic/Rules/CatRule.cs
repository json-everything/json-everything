using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("cat")]
	internal class CatRule : Rule
	{
		private readonly List<Rule> _items;

		public CatRule(Rule a, params Rule[] more)
		{
			_items = new List<Rule> { a };
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