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

	public struct Bound
	{
		public decimal Value { get; }
		public bool Inclusive { get; }

		public Bound(decimal value, bool inclusive = true)
		{
			Value = value;
			Inclusive = inclusive;
		}

		public static implicit operator Bound(int value)
		{
			return new Bound(value);
		}

		public static implicit operator Bound(decimal value)
		{
			return new Bound(value);
		}

		public static implicit operator Bound((int value, bool inclusive) pair)
		{
			return new Bound(pair.value, pair.inclusive);
		}

		public static implicit operator Bound((decimal value, bool inclusive) pair)
		{
			return new Bound(pair.value, pair.inclusive);
		}
	}

	public struct NumberRange
	{
		public Bound Minimum { get; }
		public Bound Maximum { get; }
		public bool Inverted { get; }

		public NumberRange(Bound minimum, Bound maximum, bool inverted = false)
		{
			Minimum = minimum;
			Maximum = maximum;
			Inverted = inverted;
		}

		public static IEnumerable<NumberRange> Intersection(NumberRange a, NumberRange b)
		{
			throw new NotImplementedException();
		}

		// contains: a1  b1  b2  a2 -> a1..a2
		public static IEnumerable<NumberRange> Union(NumberRange a, NumberRange b)
		{
			// a should have the lower bound. if not, then swap
			if (b.Minimum.Value < a.Minimum.Value)
				return Union(b, a);

			// disjoint: a1  a2  b1  b2 -> a1..a2, b1..b2
			if (a.Maximum.Value < b.Minimum.Value)
				return new[] {a, b};

			// tangent:  a1  a2b1  b2
			if (a.Maximum.Value == b.Minimum.Value)
			{
				// if either is inclusive -> a1..b2
				if (a.Maximum.Inclusive || b.Minimum.Inclusive)
					return new[] { new NumberRange(a.Minimum, b.Maximum) };

				// otherwise disjoint
				return new[] { a, b };
			}

			var minimum = a.Minimum.Value < b.Minimum.Value
				? a.Minimum
				: new Bound(a.Minimum.Value, a.Minimum.Inclusive || b.Minimum.Inclusive);

			// overlap:  a1  b1  a2b2 -> a1..(a2 | b2)
			if (a.Maximum.Value == b.Maximum.Value)
				return new[] {new NumberRange(minimum, new Bound(a.Maximum.Value, a.Maximum.Inclusive || b.Maximum.Inclusive))};

			// overlap:  a1  b1  a2b2 -> a1..b2
			if (a.Maximum.Value < b.Maximum.Value)
				return new[] {new NumberRange(minimum, b.Maximum)};

			// contains: a1  b1  b2  a2 -> a1..a2
			return new[] {new NumberRange(minimum, a.Maximum)};
		}
	}
}
