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
			IDataGenerator? generator;
			RequirementContext requirements;
			if (schema.BoolValue.HasValue)
			{
				if (schema.BoolValue == false)
					return GenerationResult.Fail("Boolean schema `false` allows no values");

				requirements = new RequirementContext();
				generator = Randomizer.ArrayElement(_generators);
			}
			else
			{
				requirements = GetRequirements(schema);
				var applicableGenerators = _generators
					.Where(x => requirements.Type.HasFlag(x.Type))
					.ToArray();
				generator = Randomizer.ArrayElement(applicableGenerators);
			}

			return generator.Generate(requirements);
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
