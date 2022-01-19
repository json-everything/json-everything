using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration.Generators
{
	internal class StringGenerator : IDataGenerator
	{
		public static StringGenerator Instance { get; } = new StringGenerator();

		private StringGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.String;

		public GenerationResult Generate(RequirementContext context)
		{
			var ranges = context.StringLengths ?? NumberRangeSet.NonNegative;

			var range = JsonSchemaExtensions.Randomizer.ArrayElement(ranges.Ranges.ToArray());
			var totalLength = 0;
			var words = new List<string>();
			var attempts = 0;
			while (totalLength + words.Count - 1 < range.Minimum)
			{
				var word = JsonSchemaExtensions.Randomizer.Word();
				if (totalLength + words.Count + word.Length > range.Maximum)
				{
					attempts++;
					if (attempts > 10) break;
					continue;
				}
				
				totalLength += word.Length;
				words.Add(word);				
			}

			while (totalLength + words.Count - 1 < range.Maximum)
			{
				var word = JsonSchemaExtensions.Randomizer.Word();
				if (totalLength + words.Count + word.Length >= range.Maximum) break;

				if (JsonSchemaExtensions.Randomizer.Int() % 10 == 0) break;

				totalLength += word.Length;
				words.Add(word);
			}

			var value = string.Join(" ", words);

			return GenerationResult.Success(value);
		}
	}
}