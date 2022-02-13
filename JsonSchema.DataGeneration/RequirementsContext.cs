using System;
using System.Collections.Generic;
using System.Linq;

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
		public List<decimal>? AntiMultiples { get; set; }

		public NumberRangeSet? StringLengths { get; set; }
		// https://www.ocpsoft.org/tutorials/regular-expressions/and-in-regex/
		//public List<Regex>? Patterns { get; set; }
		//public List<Regex>? AntiPatterns { get; set; }
		public string? Format { get; set; }

		public List<RequirementsContext>? SequentialItems { get; set; }
		public RequirementsContext? RemainingItems { get; set; }
		public NumberRangeSet? ItemCounts { get; set; }
		public RequirementsContext? Contains { get; set; }
		public NumberRangeSet? ContainsCounts { get; set; }

		public List<RequirementsContext>? Options { get; set; }

		public bool HasConflict { get; private set; }

		public RequirementsContext(){}

		public RequirementsContext(RequirementsContext other)
		{
			Type = other.Type;

			if (other.NumberRanges != null)
				NumberRanges = new NumberRangeSet(other.NumberRanges);
			if (other.Multiples != null)
				Multiples = other.Multiples.ToList();
			if (other.AntiMultiples != null)
				AntiMultiples = other.AntiMultiples.ToList();

			if (other.StringLengths != null)
				StringLengths = new NumberRangeSet(other.StringLengths);
			//if (other.Patterns != null)
			//	Patterns = other.Patterns.ToList();
			//if (other.AntiPatterns != null)
			//	AntiPatterns = other.AntiPatterns.ToList();

			if (other.ItemCounts != null)
				ItemCounts = new NumberRangeSet(other.ItemCounts);
			if (other.RemainingItems != null)
				RemainingItems = new RequirementsContext(other.RemainingItems);

			if (other.Options != null)
				Options = other.Options.Select(x => new RequirementsContext(x)).ToList();

			HasConflict = other.HasConflict;
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
				if (Multiples == null && AntiMultiples == null) return false;
				context.Multiples = AntiMultiples;
				context.AntiMultiples = Multiples;
				return true;
			}

			bool BreakStringLength(RequirementsContext context)
			{
				if (StringLengths == null || !StringLengths.Ranges.Any()) return false;
				context.StringLengths = StringLengths?.Invert();
				return true;
			}

			bool BreakPatterns(RequirementsContext context)
			{
				//if (Patterns == null && AntiPatterns == null) return false;
				//context.Patterns = AntiPatterns;
				//context.AntiPatterns = Patterns;
				//return true;
				return false;
			}

			var allBreakers = new Func<RequirementsContext, bool>[]
			{
				BreakType,
				BreakNumberRange,
				BreakMultiples,
				BreakStringLength,
				BreakPatterns
			};
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

			if (AntiMultiples == null)
				AntiMultiples = other.AntiMultiples;
			else if (other.AntiMultiples != null)
				AntiMultiples.AddRange(other.AntiMultiples);

			if (StringLengths == null || !StringLengths.Ranges.Any())
				StringLengths = other.StringLengths;
			else if (other.StringLengths != null)
				StringLengths *= other.StringLengths;

			//if (Patterns == null)
			//	Patterns = other.Patterns;
			//else if (other.Patterns != null)
			//	Patterns.AddRange(other.Patterns);

			//if (AntiPatterns == null)
			//	AntiPatterns = other.AntiPatterns;
			//else if (other.AntiPatterns != null)
			//	AntiPatterns.AddRange(other.AntiPatterns);

			if (ItemCounts == null || !ItemCounts.Ranges.Any())
				ItemCounts = other.ItemCounts;
			else if (other.ItemCounts != null)
				ItemCounts *= other.ItemCounts;

			if (RemainingItems == null)
				RemainingItems = other.RemainingItems;
			else if (other.RemainingItems != null)
				RemainingItems.And(other.RemainingItems);

			if (Format == null)
				Format = other.Format;
			else if (other.Format != null)
				HasConflict = true;
		}
	}
}