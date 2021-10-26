using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	}

	internal interface IRequirementsGatherer
	{
		IEnumerable<IJsonSchemaKeyword> Gather(IJsonSchemaKeyword keyword);
	}

	internal interface IValueRequirement
	{

	}

	internal class NumberRequirement : IValueRequirement
	{
		public IEnumerable<NumberRange> ValidRanges { get; }

		public NumberRequirement()
		{
			ValidRanges = new[] {new NumberRange()};
		}
	}

	public readonly struct NumberRange : IEquatable<NumberRange>
	{
		public decimal Minimum { get; }
		public decimal Maximum { get; }

		public NumberRange()
		{
			Minimum = decimal.MinValue / 2;
			Maximum = decimal.MaxValue / 2;
		}

		public NumberRange(decimal minimum, decimal maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public IEnumerable<NumberRange> Exclude(NumberRange range)
		{
			//this is wrong
			if (Minimum <= range.Maximum) yield return new NumberRange(Minimum, range.Minimum);
			if (range.Minimum <= Maximum) yield return new NumberRange(range.Maximum, Maximum);
		}

		public bool Equals(NumberRange other)
		{
			return Minimum == other.Minimum && Maximum == other.Maximum;
		}

		public override bool Equals(object? obj)
		{
			return obj is NumberRange other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Minimum.GetHashCode() * 397) ^ Maximum.GetHashCode();
			}
		}

		public static bool operator ==(NumberRange left, NumberRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(NumberRange left, NumberRange right)
		{
			return !left.Equals(right);
		}
	}
}
