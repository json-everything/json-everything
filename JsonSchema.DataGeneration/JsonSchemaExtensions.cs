using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;

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
			if (schema.BoolValue.HasValue)
			{
				if (schema.BoolValue == false)
					return GenerationResult.Fail("boolean schema `false` allows no values");

				var generator = Randomizer.ArrayElement(_generators);
				return generator.Generate(schema);
			}

			var usableGenerators = _generators;
			var typeKeywords = schema.Keywords!.OfType<TypeKeyword>().ToList();
			if (typeKeywords.Count > 1)
				return GenerationResult.Fail("invalid schema: multiple `type` keywords found");

			var typeKeyword = typeKeywords.SingleOrDefault();
			if (typeKeyword != null)
				usableGenerators = usableGenerators.Where(x => typeKeyword.Type.HasFlag(x.Type)).ToArray();

			if (!usableGenerators.Any())
				return GenerationResult.Fail("`type` keyword indicates no types are valid"); // should never happen

			var selectedGenerator = Randomizer.ArrayElement(usableGenerators);
			return selectedGenerator.Generate(schema);
		}

		private static readonly IRequirementsGatherer[] _requirementGatherers =
			typeof(JsonSchemaExtensions).Assembly
				.DefinedTypes
				.Where(x => typeof(IRequirementsGatherer).IsAssignableFrom(x) &&
				            !x.IsAbstract && !x.IsInterface)
				.Select(Activator.CreateInstance)
				.Cast<IRequirementsGatherer>()
				.ToArray();

		internal static IValueRequirement[] GetRequirements(JsonSchema schema)
		{
			// get requirements for current level
			// this will include *Of, not, etc.
			// keywords like if/then/else will need to be handled together
			// oneOf/const may direct some other value

			throw new NotImplementedException();
		}
	}

	internal interface IRequirementsGatherer
	{
		Type HandlerFor { get; }
		IEnumerable<IValueRequirement> Gather(IJsonSchemaKeyword keyword);
	}

	internal class AllOfRequirementsGatherer : IRequirementsGatherer
	{
		public Type HandlerFor => typeof(AllOfKeyword);

		public IEnumerable<IValueRequirement> Gather(IJsonSchemaKeyword keyword)
		{
			var allOfKeyword = (AllOfKeyword) keyword;
			return allOfKeyword.Schemas.SelectMany(JsonSchemaExtensions.GetRequirements);
		}
	}

	internal interface IValueRequirement
	{

	}

	internal class NumberRequirement : IValueRequirement
	{
		public IEnumerable<NumberRange> ValidRanges { get; private set; }
		public IEnumerable<decimal> Multiples { get; private set; }
		public IEnumerable<decimal> Antimultiples { get; private set; }

		public NumberRequirement Add(NumberRequirement other)
		{
			return new NumberRequirement
			{
			};
		}

		public NumberRequirement Remove(NumberRequirement other)
		{
			throw new NotImplementedException();
		}
	}
}
