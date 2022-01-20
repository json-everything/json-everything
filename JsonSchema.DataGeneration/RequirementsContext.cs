using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Json.Schema.DataGeneration
{
	internal class RequirementsContext
	{
		public const SchemaValueType AllTypes =
			SchemaValueType.Array |
			SchemaValueType.Boolean |
			SchemaValueType.Integer |
			SchemaValueType.Null |
			SchemaValueType.Number |
			SchemaValueType.Object |
			SchemaValueType.String;

		public SchemaValueType? Type { get; set; }

		public NumberRangeSet? NumberRanges { get; set; }
		public List<decimal>? Multiples { get; set; }
		public List<decimal>? Antimultiples { get; set; }

		public NumberRangeSet? StringLengths { get; set; }
		// https://www.ocpsoft.org/tutorials/regular-expressions/and-in-regex/
		public List<Regex>? Patterns { get; set; }
		public List<Regex>? AntiPatterns { get; set; }
		public string? Format { get; set; }

		public List<RequirementsContext>? Options { get; set; }

		public RequirementsContext(){}

		public RequirementsContext(RequirementsContext other)
		{
			Type = other.Type;
			if (other.NumberRanges != null)
				NumberRanges = new NumberRangeSet(other.NumberRanges);
			if (other.Multiples != null)
				Multiples = other.Multiples.ToList();
			if (other.Antimultiples != null)
				Antimultiples = other.Antimultiples.ToList();
		}

		public IEnumerable<RequirementsContext> GetAllVariations()
		{
			RequirementsContext CreateVariation(RequirementsContext option)
			{
				var variation = new RequirementsContext(this);
				variation.And(option);
				return variation;
			}

			if (Options == null)
				yield return this;
			else
			{
				using var enumerator = Options.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (enumerator.Current!.Options != null)
					{
						foreach (var variation in enumerator.Current.GetAllVariations())
						{
							yield return CreateVariation(variation);
						}
					}
					else
						yield return CreateVariation(enumerator.Current);
				}
			}
		}

		// Create a requirements object that doesn't meet this context's requirement
		// Only need to break one requirement for this to work, not all
		public RequirementsContext Break()
		{
			bool BreakType(RequirementsContext context)
			{
				if (Type == null) return false;
				context.Type = ~AllTypes ^ ~Type;
				return true;
			}

			bool BreakNumberRange(RequirementsContext context)
			{
				if (NumberRanges == null || !NumberRanges.Ranges.Any()) return false;
				context.NumberRanges = NumberRanges?.Invert();
				return true;
			}

			bool BreakMultiples(RequirementsContext context)
			{
				if (context.Multiples == null && context.Antimultiples == null) return false;
				context.Multiples = Antimultiples;
				context.Antimultiples = Multiples;
				return true;
			}

			var allBreakers = new Func<RequirementsContext, bool>[] {BreakType, BreakNumberRange, BreakMultiples};
			var breakers = JsonSchemaExtensions.Randomizer.Shuffle(allBreakers);

			var broken = new RequirementsContext(this);
			using var enumerator = breakers.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current(broken)) ;

			return broken;
		}

		public void And(RequirementsContext other)
		{
			if (Type == null)
				Type = other.Type;
			else if (other.Type != null)
				Type &= other.Type;

			if (NumberRanges == null || !NumberRanges.Ranges.Any())
				NumberRanges = other.NumberRanges;
			else if (other.NumberRanges != null)
				NumberRanges *= other.NumberRanges;

			if (Multiples == null)
				Multiples = other.Multiples;
			else if (other.Multiples != null)
				Multiples.AddRange(other.Multiples);

			if (Antimultiples == null)
				Antimultiples = other.Antimultiples;
			else if (other.Antimultiples != null)
				Antimultiples.AddRange(other.Antimultiples);
		}
	}
}