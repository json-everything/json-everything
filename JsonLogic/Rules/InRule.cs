using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("in")]
	internal class InRule : Rule
	{
		private readonly Rule _test;
		private readonly Rule _source;

		public InRule(Rule test, Rule source)
		{
			_test = test;
			_source = source;
		}
	
		public override JsonElement Apply(JsonElement data)
		{
			var test = _test.Apply(data);
			var source = _source.Apply(data);

			if (source.ValueKind == JsonValueKind.String)
			{
				var stringTest = test.Stringify();
				var stringSource = source.GetString();

				if (stringTest == null || stringSource == null)
					throw new JsonLogicException($"Cannot check string for {test.ValueKind}.");

				return (!string.IsNullOrEmpty(stringTest) && stringSource.Contains(stringTest)).AsJsonElement();
			}

			if (source.ValueKind == JsonValueKind.Array)
			{
				var items = source.EnumerateArray();
				return items.Any(i => i.IsEquivalentTo(test)).AsJsonElement();
			}

			throw new JsonLogicException($"Cannot apply `in` to {test.ValueKind} and {source.ValueKind}.");
		}
	}
}