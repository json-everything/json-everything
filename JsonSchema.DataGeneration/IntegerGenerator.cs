using System;
using System.Linq;

namespace Json.Schema.DataGeneration;

public class IntegerGenerator : IDataGenerator
{
	public static IntegerGenerator Instance { get; } = new IntegerGenerator();

	private IntegerGenerator() { }

	public SchemaValueType Type => SchemaValueType.Integer;

	public GenerationResult Generate(JsonSchema schema)
	{
		var minimum = schema.Keywords.OfType<MinimumKeyword>().FirstOrDefault()?.Value;
		var minValue = minimum.HasValue ? (long) Math.Ceiling(minimum.Value) : (long.MinValue / 2);
		var maximum = schema.Keywords.OfType<MaximumKeyword>().FirstOrDefault()?.Value;
		var maxValue = maximum.HasValue ? (long) Math.Floor(maximum.Value) : (long.MaxValue / 2);
		var multipleOf = schema.Keywords.OfType<MultipleOfKeyword>().FirstOrDefault()?.Value ?? 1;

		var value = JsonSchemaExtensions.Randomizer.Decimal() * (maxValue - minValue) + minValue;
		var factor = (long) Lcm(multipleOf, 1);
		value = Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;

		return GenerationResult.Success(value);
	}

	private static decimal Gcf(decimal a, decimal b)
	{
		while (b != 0)
		{
			decimal temp = b;
			b = a % b;
			a = temp;
		}
		return a;
	}

	private static decimal Lcm(decimal a, decimal b)
	{
		return (a / Gcf(a, b)) * b;
	}
}