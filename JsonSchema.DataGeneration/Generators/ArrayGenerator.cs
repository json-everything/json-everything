using System;
using System.Collections.Generic;
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
		public static uint DefaultMinContains { get; set; } = 1;
		public static uint DefaultMaxContains { get; set; } = 10;

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

			var containsCount = 0;
			if (context.Contains != null)
			{
				var minContains = DefaultMinContains;
				var maxContains = Math.Min(maxItems, DefaultMaxContains);
				if (context.ContainsCounts != null)
				{
					var numberRange = JsonSchemaExtensions.Randomizer.ArrayElement(context.ContainsCounts.Ranges.ToArray());
					if (numberRange.Minimum.Value != NumberRangeSet.MinRangeValue)
						minContains = (uint) numberRange.Minimum.Value;
					if (numberRange.Maximum.Value != NumberRangeSet.MaxRangeValue)
						maxContains = (uint) numberRange.Maximum.Value;
				}

				containsCount = (int) JsonSchemaExtensions.Randomizer.UInt(minContains, maxContains);
				if (containsCount > itemCount)
				{
					if (itemCount < minContains)
					{
						if (minContains > minItems)
							return GenerationResult.Fail("minContains is greater than minItems");
						itemCount = containsCount;
					}
					else
						containsCount = itemCount;
				}
			}

			var containsIndices = JsonSchemaExtensions.Randomizer
				.ArrayElements(Enumerable.Range(0, itemCount).ToArray(), containsCount)
				.OrderBy(x => x)
				.ToArray();

			var itemGenerationResults = new List<GenerationResult>();

			// check sequential items
			var sequenceCount = 0;

			var remainingRequirements = context.RemainingItems ?? new RequirementsContext();
			int currentContainsIndex = 0;
			for (int i = sequenceCount; i < itemCount; i++)
			{
				var itemRequirement = remainingRequirements;
				if (containsCount > 0 && currentContainsIndex < containsIndices.Length && i == containsIndices[currentContainsIndex])
				{
					itemRequirement = new RequirementsContext(itemRequirement);
					itemRequirement.And(context.Contains!);
					currentContainsIndex++;
				}

				itemGenerationResults.Add(itemRequirement.GenerateData());
			}

			return itemGenerationResults.All(x => x.IsSuccess)
				? GenerationResult.Success(itemGenerationResults.Select(x => x.Result).AsJsonElement())
				: GenerationResult.Fail(itemGenerationResults);
		}
	}
}