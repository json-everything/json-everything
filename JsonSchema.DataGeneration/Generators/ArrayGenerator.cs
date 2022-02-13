using System.Linq;
using Json.More;

namespace Json.Schema.DataGeneration.Generators
{
	internal class ArrayGenerator : IDataGenerator
	{
		public static ArrayGenerator Instance { get; } = new ArrayGenerator();

		// TODO: move these to a public settings object
		public static uint DefaultMinItems { get; set; } = 0;
		public static uint DefaultMaxItems { get; set; } = 10;

		private ArrayGenerator() { }

		public SchemaValueType Type => SchemaValueType.Array;

		public GenerationResult Generate(RequirementsContext context)
		{
			var minItems = DefaultMinItems;
			var maxItems = DefaultMaxItems;
			if (context.ItemCounts != null)
			{
				var numberRange = JsonSchemaExtensions.Randomizer.ArrayElement(context.ItemCounts.Ranges.ToArray());
				if (numberRange.Minimum.Value != NumberRangeSet.MinRangeValue)
					minItems = (uint) numberRange.Minimum.Value;
				if (numberRange.Maximum.Value != NumberRangeSet.MaxRangeValue)
					maxItems = (uint) numberRange.Maximum.Value;
			}
			var itemCount = (int) JsonSchemaExtensions.Randomizer.UInt(minItems, maxItems);

			if (context.RemainingItems != null)
			{
				var itemGenerationResults = Enumerable.Range(0, itemCount).Select(x => context.RemainingItems.GenerateData()).ToArray();
				if (itemGenerationResults.All(x => x.IsSuccess))
					return GenerationResult.Success(itemGenerationResults.Select(x => x.Result).AsJsonElement());
				else
					return GenerationResult.Fail(itemGenerationResults);
			}

			return GenerationResult.Fail("Could not generate items for an array");
		}
	}
}