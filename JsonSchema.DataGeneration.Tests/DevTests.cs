using System;
using System.Linq;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public class DevTests
	{
		[Test]
		public void Test()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(0)
				.Maximum(100)
				.Not(new JsonSchemaBuilder().MultipleOf(3));

			var result = schema.GenerateData();

			Console.WriteLine(result.Result.ToJsonString());

			Assert.IsTrue(schema.Validate(result.Result).IsValid);
		}
	}

	public class NumberRangeTests
	{
		[Test]
		public void OmitRangeEntirelyLessThan()
		{
			var start = new NumberRange(-10, 10);
			var omit = new NumberRange(-30, -20);

			var results = start.Exclude(omit).ToArray();

			Assert.AreEqual(1, results.Length);
			Assert.AreEqual(start, results[0]);
		}

		[Test]
		public void RandomNumberNotAMultiple()
		{
			var primes = new[] {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47};

			var low = 0m;
			var high = 100m;
			var not = new[] {4m, 5m, 6m};
			var target = not.Zip(primes).Take(not.Length).Select(x => x.First * x.Second).Sum();
			var value = Multiple(target, low, high);

			Console.WriteLine(target);
			Console.WriteLine(value);
			Assert.IsTrue(low <= value);
			Assert.IsTrue(value <= high);
			foreach (var div in not)
			{
				Assert.IsTrue(value % div != 0);
			}
		}

		private static decimal Multiple(decimal of, decimal min, decimal max)
		{
			var value = new Bogus.Randomizer().Decimal() * (max - min) + min;
			var factor = (long) Lcm(of, 1);
			value = Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
			
			Console.WriteLine($"random: {value}");

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

		private decimal Gcf(decimal[] arr)
		{
			var result = arr[0];
			for (int i = 1; i < arr.Length; i++)
				result = Gcf(arr[i], result);

			return result;
		}

		private static decimal Lcm(decimal a, decimal b)
		{
			return (a / Gcf(a, b)) * b;
		}

		private decimal Lcm(decimal[] arr)
		{
			var result = arr[0];
			for (int i = 1; i < arr.Length; i++)
				result = Lcm(arr[i], result);

			return result;
		}
	}
}