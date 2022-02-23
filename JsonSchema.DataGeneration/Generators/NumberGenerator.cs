using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration.Generators
{
	internal class NumberGenerator : IDataGenerator
	{
		public static NumberGenerator Instance { get; } = new NumberGenerator();

		private NumberGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.Number;

		public GenerationResult Generate(RequirementsContext context)
		{
			context.NumberRanges ??= NumberRangeSet.Full;
			
			// we have no requirements, so use the full range
			if (!context.NumberRanges.Ranges.Any())
				context.NumberRanges = NumberRangeSet.Full;

			var availableRanges = context.NumberRanges.Ranges.ToList();

			while (availableRanges.Any())
			{
				var selectedIndex = JsonSchemaExtensions.Randomizer.Int(0, availableRanges.Count - 1);
				var selectedRange = availableRanges[selectedIndex];

				var minValue = selectedRange.Minimum.Value;
				var isInclusiveMin = selectedRange.Minimum.Inclusive;
				var maxValue = selectedRange.Maximum.Value;
				var isInclusiveMax = selectedRange.Maximum.Inclusive;
				decimal? multipleOf = null;
				if (context.Multiples != null && context.Multiples.Any())
				{
					if (context.Multiples.Count == 1)
						multipleOf = context.Multiples.Single();
					else
						multipleOf = Lcm(context.Multiples);
				}

				decimal value;
				int attempts = 0;
				do
				{
					value = JsonSchemaExtensions.Randomizer.Decimal(minValue, maxValue);
					if (multipleOf.HasValue)
					{
						var factor = multipleOf.Value;
						value = Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
					}

					if (!selectedRange.Contains(value))
					{
						attempts++;
						if (attempts > 5) break;
					}

				} while (context.AntiMultiples != null && context.AntiMultiples.Any(x => value % x == 0));

				var meetsMinimum = isInclusiveMin ? minValue <= value : minValue < value;
				var meetsMaximum = isInclusiveMax ? value <= maxValue : value < maxValue;

				if (meetsMinimum && meetsMaximum)
					return GenerationResult.Success(value);

				availableRanges.RemoveAt(selectedIndex);
			}

			return GenerationResult.Fail("Could not generate a numeric value that met all requirements.");
		}

		private static decimal Lcm(IEnumerable<decimal> values)
		{
			var gcd = Gcd(values);
			var product = values.Aggregate(1m, (c, x) => c * x);

			return product / gcd;
		}

		private static decimal Gcd(IEnumerable<decimal> values)
		{
			var current = values.First();
			foreach (var value in values.Skip(1))
			{
				current = Gcd(current, value);
			}

			return current;
		}

		private static decimal Gcd(decimal a, decimal b)
		{
			while (true)
			{
				if (a < b)
				{
					(a, b) = (b, a);
					continue;
				}

				// base case
				if (b == 0) return a;

				var a1 = a;
				a = b;
				b = a1 - Math.Floor(a1 / b) * b;
			}
		}
	}
}