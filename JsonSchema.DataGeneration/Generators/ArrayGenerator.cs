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
				if (!context.ItemCounts.Ranges.Any())
					return GenerationResult.Fail("No valid item counts possible");

				var numberRange = JsonSchemaExtensions.Randomizer.ArrayElement(context.ItemCounts.Ranges.ToArray());
				if (numberRange.Minimum.Value != NumberRangeSet.MinRangeValue)
					minItems = (uint) (numberRange.Minimum.Inclusive
						? numberRange.Minimum.Value
						: numberRange.Minimum.Value + 1);
				if (numberRange.Maximum.Value != NumberRangeSet.MaxRangeValue)
					maxItems = (uint) (numberRange.Maximum.Inclusive
						? numberRange.Maximum.Value
						: numberRange.Maximum.Value - 1);
			}

			var itemCount = (int) JsonSchemaExtensions.Randomizer.UInt(minItems, maxItems);
			var sequenceCount = (uint?) context.SequentialItems?.Count ?? 0;
			var containsCount = 0;
			if (context.Contains != null)
			{
				var minContains = DefaultMinContains;
				var maxContains = Math.Min(maxItems - sequenceCount, DefaultMaxContains + minContains);
				if (context.ContainsCounts != null)
				{
					var numberRange = JsonSchemaExtensions.Randomizer.ArrayElement(context.ContainsCounts.Ranges.ToArray());
					if (numberRange.Minimum.Value != NumberRangeSet.MinRangeValue)
						minContains = (uint) numberRange.Minimum.Value;
					if (numberRange.Maximum.Value != NumberRangeSet.MaxRangeValue)
						maxContains = (uint) numberRange.Maximum.Value;
				}

				// some simple checks to ensure an instance can be generated
				if (minContains > maxContains)
					return GenerationResult.Fail("minContains is greater than maxContains");
				if (minContains > maxItems - sequenceCount)
					return GenerationResult.Fail("minContains is greater than maxItems less sequential items count");

				containsCount = (int) JsonSchemaExtensions.Randomizer.UInt(minContains, maxContains);
				if (itemCount < containsCount) 
					itemCount = containsCount;
			}

			var itemGenerationResults = new List<GenerationResult>();

			var currentSequenceIndex = 0;
			if (context.SequentialItems != null)
			{
				while (currentSequenceIndex < itemCount && currentSequenceIndex < context.SequentialItems.Count)
				{
					var itemRequirement = context.SequentialItems[currentSequenceIndex];
					itemGenerationResults.Add(itemRequirement.GenerateData());
					currentSequenceIndex++;
				}
			}

			var possibleIndices = Enumerable.Range(currentSequenceIndex, itemCount - currentSequenceIndex).ToArray();
			var containsIndices = JsonSchemaExtensions.Randomizer
				.ArrayElements(possibleIndices, Math.Min(possibleIndices.Length, containsCount))
				.OrderBy(x => x)
				.ToArray();

			var remainingRequirements = context.RemainingItems ?? new RequirementsContext();
			if (!remainingRequirements.IsFalse)
			{
				int currentContainsIndex = 0;
				for (int i = currentSequenceIndex; i < itemCount; i++)
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
			}
			else if (itemGenerationResults.Count < minItems)
				return GenerationResult.Fail("Could not generate sufficient items to meet requirements");

			return itemGenerationResults.All(x => x.IsSuccess)
				? GenerationResult.Success(itemGenerationResults.Select(x => x.Result).AsJsonElement())
				: GenerationResult.Fail(itemGenerationResults);
		}
	}
}