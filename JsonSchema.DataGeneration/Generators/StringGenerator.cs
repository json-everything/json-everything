using System;
using System.Linq;
using Fare;

namespace Json.Schema.DataGeneration.Generators
{
	internal class StringGenerator : IDataGenerator
	{
		private const int _maxStringLength = 100;

		public static StringGenerator Instance { get; } = new StringGenerator();

		private StringGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.String;

		public GenerationResult Generate(RequirementsContext context)
		{
			var ranges = context.StringLengths ?? NumberRangeSet.NonNegative;
			ranges = ranges.Floor(0).Ceiling(_maxStringLength);
			var range = JsonSchemaExtensions.Randomizer.ArrayElement(ranges.Ranges.ToArray());
			var rangeRegex = $".{{{range.Minimum.Value},{range.Maximum.Value}}}";
			
			string overallRegex = string.Empty;

			if (context.Patterns != null)
				overallRegex += HelperExtensions.Require(context.Patterns.Select(x => x.ToString()));

			if (context.AntiPatterns != null)
				overallRegex += HelperExtensions.Forbid(context.AntiPatterns.Select(x => x.ToString()));

			overallRegex += rangeRegex;
			overallRegex = $"^{overallRegex}$";

			Console.WriteLine(overallRegex);

			return GenerationResult.Success(new Xeger(overallRegex).Generate());
		}
	}
}