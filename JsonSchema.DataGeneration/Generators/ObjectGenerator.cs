using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Json.More;

namespace Json.Schema.DataGeneration.Generators
{
	internal class ObjectGenerator : IDataGenerator
	{
		public static ObjectGenerator Instance { get; } = new ObjectGenerator();
		private static readonly Faker _faker = new Faker();

		// TODO: move these to a public settings object
		public static uint DefaultMinProperties { get; set; } = 0;
		public static uint DefaultMaxProperties { get; set; } = 10;
		public static uint DefaultMinContains { get; set; } = 1;
		public static uint DefaultMaxContains { get; set; } = 10;

		private ObjectGenerator() { }

		public SchemaValueType Type => SchemaValueType.Object;

		public GenerationResult Generate(RequirementsContext context)
		{
			var minProperties = DefaultMinProperties;
			var maxProperties = DefaultMaxProperties;
			if (context.PropertyCounts != null)
			{
				if (!context.PropertyCounts.Ranges.Any())
					return GenerationResult.Fail("No valid property counts possible");

				var numberRange = JsonSchemaExtensions.Randomizer.ArrayElement(context.PropertyCounts.Ranges.ToArray());
				if (numberRange.Minimum.Value != NumberRangeSet.MinRangeValue)
					minProperties = (uint) (numberRange.Minimum.Inclusive
						? numberRange.Minimum.Value
						: numberRange.Minimum.Value + 1);
				if (numberRange.Maximum.Value != NumberRangeSet.MaxRangeValue)
					maxProperties = (uint) (numberRange.Maximum.Inclusive
						? numberRange.Maximum.Value
						: numberRange.Maximum.Value - 1);
			}

			var propertyCount = (int) JsonSchemaExtensions.Randomizer.UInt(minProperties, maxProperties);
			var containsCount = 0;
			if (context.Contains != null)
			{
				var minContains = DefaultMinContains;
				var maxContains = Math.Min(maxProperties, DefaultMaxContains + minContains);
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
				if (minContains > maxProperties)
					return GenerationResult.Fail("minContains is greater than maxItems less property count");

				containsCount = (int) JsonSchemaExtensions.Randomizer.UInt(minContains, maxContains);
				if (propertyCount < containsCount)
					propertyCount = containsCount;
			}

			var propertyGenerationResults = new Dictionary<string, GenerationResult>();
			var definedPropertyNames = new List<string>();
			var remainingPropertyCount = propertyCount;
			if (context.RequiredProperties != null)
			{
				definedPropertyNames.AddRange(context.RequiredProperties);
				remainingPropertyCount -= context.RequiredProperties.Count;
			}
			if (context.Properties != null)
			{
				var propertyNames = context.Properties.Keys.Except(definedPropertyNames).ToArray();
				if (propertyNames.Length != 0)
				{
					propertyNames = JsonSchemaExtensions.Randomizer.ArrayElements(propertyNames, Math.Min(definedPropertyNames.Count, remainingPropertyCount));
					definedPropertyNames.AddRange(propertyNames);
					remainingPropertyCount -= propertyNames.Length;
				}
			}

			var remainingProperties = context.RemainingProperties ?? new RequirementsContext();
			if (remainingProperties.IsFalse)
				remainingPropertyCount = 0;
			remainingPropertyCount = Math.Max(0, remainingPropertyCount);
			var otherPropertyNames = remainingPropertyCount == 0
				? Array.Empty<string>()
				: _faker.Lorem.Sentence(remainingPropertyCount * 2).Split(' ')
					.Distinct()
					.Take(remainingPropertyCount)
					.ToArray();
			var allPropertyNames = definedPropertyNames.Concat(otherPropertyNames).ToArray();
			var containsProperties = JsonSchemaExtensions.Randomizer
				.ArrayElements(allPropertyNames, Math.Min(allPropertyNames.Length, containsCount))
				.ToArray();

			int currentContainsIndex = 0;
			foreach (var propertyName in allPropertyNames)
			{
				if (context.Properties?.TryGetValue(propertyName, out var propertyRequirement) != true)
					propertyRequirement = context.RemainingProperties ?? new RequirementsContext();
				if (containsCount > 0 && currentContainsIndex < containsProperties.Length)
				{
					propertyRequirement = new RequirementsContext(propertyRequirement!);
					propertyRequirement.And(context.Contains!);
					currentContainsIndex++;
				}

				propertyGenerationResults.Add(propertyName, propertyRequirement!.GenerateData());
			}

			return propertyGenerationResults.All(x => x.Value.IsSuccess)
				? GenerationResult.Success(propertyGenerationResults.ToDictionary(x => x.Key, x => x.Value.Result).AsJsonElement())
				: GenerationResult.Fail(propertyGenerationResults.Values);
		}
	}
}