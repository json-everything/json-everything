using System;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Schema.DataGeneration;

public class NumberGenerator : IDataGenerator
{
	public static NumberGenerator Instance { get; } = new NumberGenerator();

	private NumberGenerator() { }

	public SchemaValueType Type => SchemaValueType.Number;

	public GenerationResult Generate(JsonSchema schema)
	{
		var minValue = schema.Keywords.OfType<MinimumKeyword>().FirstOrDefault()?.Value ?? (decimal.MinValue / 2);
		var maxValue = schema.Keywords.OfType<MaximumKeyword>().FirstOrDefault()?.Value ?? (decimal.MaxValue / 2);
		var multipleOf = schema.Keywords.OfType<MultipleOfKeyword>().FirstOrDefault()?.Value;

		var value = JsonSchemaExtensions.Randomizer.Decimal() * (maxValue - minValue) + minValue;
		if (multipleOf.HasValue)
		{
			var factor = multipleOf.Value;
			value = Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
		}

		return GenerationResult.Success(value);
	}
}