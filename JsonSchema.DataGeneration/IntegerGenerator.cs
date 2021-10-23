using System;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Schema.DataGeneration;

public class IntegerGenerator : IDataGenerator
{
	public static IntegerGenerator Instance { get; } = new IntegerGenerator();

	private IntegerGenerator() { }

	public SchemaValueType Type => SchemaValueType.Integer;

	public JsonElement Generate(JsonSchema schema)
	{

		var minimum = schema.Keywords.OfType<MinimumKeyword>().FirstOrDefault()?.Value;
		var minValue = minimum.HasValue ? (long) Math.Ceiling(minimum.Value) : (long.MinValue / 2);
		var maximum = schema.Keywords.OfType<MaximumKeyword>().FirstOrDefault()?.Value;
		var maxValue = maximum.HasValue ? (long) Math.Floor(maximum.Value) : (long.MaxValue / 2);
		var multipleOf = schema.Keywords.OfType<MultipleOfKeyword>().FirstOrDefault()?.Value;

		var value = JsonSchemaExtensions.Randomizer.Decimal() * (maxValue - minValue) + minValue;
		if (multipleOf.HasValue)
		{
			var factor = (long) Lcm(multipleOf.Value, 1);
			value = Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
		}

		return value.AsJsonElement();
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