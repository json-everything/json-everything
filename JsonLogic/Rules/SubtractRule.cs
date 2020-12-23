using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("-")]
	internal class SubtractRule : Rule
	{
		private readonly List<Rule> _items;

		public SubtractRule(Rule a, params Rule[] more)
		{
			_items = new List<Rule> { a };
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