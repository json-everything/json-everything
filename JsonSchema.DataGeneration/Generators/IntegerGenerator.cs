using System;
using System.Linq;

namespace Json.Schema.DataGeneration.Generators
{
	internal class IntegerGenerator : IDataGenerator
	{
		public static IntegerGenerator Instance { get; } = new IntegerGenerator();

		private IntegerGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.Integer;

		public GenerationResult Generate(RequirementContext context)
		{
			JsonSchema schema = null;

			var minimum = schema.Keywords?.OfType<MinimumKeyword>().FirstOrDefault()?.Value;
			var minValue = minimum.HasValue ? (long) Math.Ceiling(minimum.Value) : -1000;
			var maximum = schema.Keywords?.OfType<MaximumKeyword>().FirstOrDefault()?.Value;
			var maxValue = maximum.HasValue ? (long) Math.Floor(maximum.Value) : 1000;
			var multipleOf = schema.Keywords?.OfType<MultipleOfKeyword>().FirstOrDefault()?.Value ?? 1;

			var value = GetValue(minValue, maxValue, new[] {multipleOf}, new decimal[] { });

			return value.HasValue
				? GenerationResult.Success(value.Value)
				: GenerationResult.Fail("Cannot generate random with that meets all requirements");
		}

		private static long? GetValue(long lowerBound, long upperBound, decimal[] requireMultiple, decimal[] requireNotMultiple)
		{
			var period = (int) Lcm(requireNotMultiple.Concat(requireMultiple).Concat(new[] {1m}).ToArray());
			if (period == 1) return JsonSchemaExtensions.Randomizer.Long(lowerBound, upperBound);

			var range = upperBound - lowerBound;
			var lowBound = range < period ? lowerBound % period : lowerBound;
			var highBound = range < period ? upperBound % period : upperBound;
			var multipleOf = Lcm(requireMultiple);
			var offset = 0;
			if (requireNotMultiple.Length != 0)
			{
				var nonMultiples = Enumerable.Range(0, period)
					.Where(x => requireNotMultiple.All(n => x % n != 0) &&
					            x % multipleOf == 0 &&
					            lowBound <= x && x <= highBound)
					.ToArray();
				if (nonMultiples.Length == 0) return null;
				offset = nonMultiples[JsonSchemaExtensions.Randomizer.Int(0, nonMultiples.Length - 1)];
			}

			var scaledRange = (int) (upperBound - lowerBound) / period;
			var lowOffset = (int) (lowerBound / period) * period;
			var value = JsonSchemaExtensions.Randomizer.Int(0, scaledRange) * period + lowOffset + offset;
			if (value > upperBound) value -= period;
			if (lowerBound > value) value += period;

			return value;
		}

		private static decimal Gcf(decimal a, decimal b)
		{
			while (b != 0)
			{
				var temp = b;
				b = a % b;
				a = temp;
			}

			return a;
		}

		private static decimal Lcm(decimal a, decimal b)
		{
			return (a / Gcf(a, b)) * b;
		}

		private static decimal Lcm(decimal[] nums)
		{
			if (nums.Length == 0) return 1;
			if (nums.Length == 1) return nums[0];
			var first = Lcm(nums[0], nums[1]);
			return nums.Skip(2).Aggregate(first, Lcm);
		}
	}
}