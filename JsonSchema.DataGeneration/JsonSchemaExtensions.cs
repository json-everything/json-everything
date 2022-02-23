using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Json.Schema.DataGeneration.Generators;

namespace Json.Schema.DataGeneration
{
	/// <summary>
	/// Provides extension methods for <see cref="JsonSchema"/> to generate sample data.
	/// </summary>
	public static class JsonSchemaExtensions
	{
		private static readonly IPrioritizedDataGenerator[] _priorityGenerators =
		{
			ConstGenerator.Instance,
			EnumGenerator.Instance
		};
		// simplest weighting is just to duplicate entries
		private static readonly IDataGenerator[] _generators =
		{
			ObjectGenerator.Instance,
			ArrayGenerator.Instance,
			IntegerGenerator.Instance,
			IntegerGenerator.Instance,
			NumberGenerator.Instance,
			NumberGenerator.Instance,
			StringGenerator.Instance,
			StringGenerator.Instance,
			BooleanGenerator.Instance,
			BooleanGenerator.Instance,
			NullGenerator.Instance,
			NullGenerator.Instance,
		};

		internal static readonly Randomizer Randomizer = new Randomizer();

		/// <summary>
		/// Attempts to generate sample data that meets the requirements of the schema.
		/// </summary>
		/// <param name="schema">The schema.</param>
		/// <returns>A result object indicating success and containing the result or error message.</returns>
		public static GenerationResult GenerateData(this JsonSchema schema)
		{
			var requirements = GetRequirements(schema);

			return requirements.GenerateData();
		}

		internal static GenerationResult GenerateData(this RequirementsContext requirements)
		{
			if (requirements.IsFalse)
				return GenerationResult.Fail("Boolean schema `false` allows no values");

			foreach (var variation in Randomizer.Shuffle(requirements.GetAllVariations()))
			{
				if (variation.HasConflict) continue;

				var priorityGenerator = _priorityGenerators.FirstOrDefault(x => x.Applies(variation));
				if (priorityGenerator != null)
					return priorityGenerator.Generate(variation);

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

		internal static RequirementsContext GetRequirements(this JsonSchema schema)
		{
			var context = new RequirementsContext();
			foreach (var gatherer in _requirementsGatherers)
			{
				gatherer.AddRequirements(context, schema);
			}

			return context;
		}
	}
}
