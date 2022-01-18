using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Json.Schema.DataGeneration.Generators;

namespace Json.Schema.DataGeneration
{
	public static class JsonSchemaExtensions
	{
		private static readonly IDataGenerator[] _generators =
		{
			ObjectGenerator.Instance,
			ArrayGenerator.Instance,
			IntegerGenerator.Instance,
			NumberGenerator.Instance,
			StringGenerator.Instance,
			BooleanGenerator.Instance,
			NullGenerator.Instance
		};

		internal static readonly Randomizer Randomizer = new Randomizer();

		public static GenerationResult GenerateData(this JsonSchema schema)
		{
			RequirementContext requirements;
			if (schema.BoolValue.HasValue)
			{
				if (schema.BoolValue == false)
					return GenerationResult.Fail("Boolean schema `false` allows no values");

				requirements = new RequirementContext();
			}
			else
				requirements = GetRequirements(schema);

			foreach (var variation in Randomizer.Shuffle(requirements.GetAllVariations()))
			{
				var applicableGenerators = _generators
					.Where(x => !variation.Type.HasValue || variation.Type.Value.HasFlag(x.Type))
					.ToArray();
				if (applicableGenerators.Length == 0) continue;

				var generator = Randomizer.ArrayElement(applicableGenerators);
				var result = generator.Generate(variation);
				if (result.IsSuccess) return result;
			}

			return GenerationResult.Fail("Could not generate data that validates against the schema.");
		}

		private static readonly IEnumerable<IRequirementsGatherer> _requirementsGatherers =
			typeof(IRequirementsGatherer).Assembly
				.DefinedTypes
				.Where(x => typeof(IRequirementsGatherer).IsAssignableFrom(x) &&
				            !x.IsAbstract &&
				            !x.IsInterface)
				.Select(Activator.CreateInstance)
				.Cast<IRequirementsGatherer>()
				.ToList();

		internal static RequirementContext GetRequirements(this JsonSchema schema)
		{
			var context = new RequirementContext();
			foreach (var gatherer in _requirementsGatherers)
			{
				gatherer.AddRequirements(context, schema);
			}

			return context;
		}
	}
}
