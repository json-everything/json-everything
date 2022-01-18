using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration
{
	internal class RequirementContext
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
		public List<RequirementContext>? Options { get; set; }

		public RequirementContext(){}

		public RequirementContext(RequirementContext other)
		{
			Type = other.Type;
			if (other.NumberRanges != null)
				NumberRanges = new NumberRangeSet(other.NumberRanges);
			if (other.Multiples != null)
				Multiples = other.Multiples.ToList();
			if (other.Antimultiples != null)
				Antimultiples = other.Antimultiples.ToList();
		}

		public IEnumerable<RequirementContext> GetAllVariations()
		{
			RequirementContext CreateVariation(RequirementContext option)
			{
				var variation = new RequirementContext(this);
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
		public RequirementContext Break()
		{
			bool BreakType(RequirementContext context)
			{
				if (Type == null) return false;
				context.Type = ~AllTypes ^ ~Type;
				return true;
			}

			bool BreakNumberRange(RequirementContext context)
			{
				if (NumberRanges == null || !NumberRanges.Ranges.Any()) return false;
				context.NumberRanges = NumberRanges?.Invert();
				return true;
			}

			bool BreakMultiples(RequirementContext context)
			{
				if (context.Multiples == null && context.Antimultiples == null) return false;
				context.Multiples = Antimultiples;
				context.Antimultiples = Multiples;
				return true;
			}

			var allBreakers = new Func<RequirementContext, bool>[] {BreakType, BreakNumberRange, BreakMultiples};
			var breakers = JsonSchemaExtensions.Randomizer.Shuffle(allBreakers);

			var broken = new RequirementContext(this);
			using var enumerator = breakers.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current(broken)) ;

			return broken;
		}

		public void And(RequirementContext other)
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