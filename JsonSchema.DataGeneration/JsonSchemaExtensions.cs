using System.Linq;
using Bogus;

namespace Json.Schema.DataGeneration
{
	public static class JsonSchemaExtensions
	{
		private static readonly IDataGenerator[] _generators =
		{
			ObjectGenerator.Instance,
			ArrayGenerator.Instance,
			IntegerGenerator.Instance,
			NumberGenerator.Instance,
			StringGenerator.Instance,
			BooleanGenerator.Instance,
			NullGenerator.Instance
		};

		internal static readonly Randomizer Randomizer = new Randomizer();

		public static GenerationResult GenerateData(this JsonSchema schema)
		{
			if (schema.BoolValue.HasValue)
			{
				if (schema.BoolValue == false)
					return GenerationResult.Fail("boolean schema `false` allows no values");

				var generator = Randomizer.ArrayElement(_generators);
				return generator.Generate(schema);
			}

			var usableGenerators = _generators;
			var typeKeywords = schema.Keywords!.OfType<TypeKeyword>().ToList();
			if (typeKeywords.Count > 1)
				return GenerationResult.Fail("invalid schema: multiple `type` keywords found");

			var typeKeyword = typeKeywords.SingleOrDefault();
			if (typeKeyword != null)
				usableGenerators = usableGenerators.Where(x => typeKeyword.Type.HasFlag(x.Type)).ToArray();

			if (!usableGenerators.Any())
				return GenerationResult.Fail("`type` keyword indicates no types are valid"); // should never happen

			var selectedGenerator = Randomizer.ArrayElement(usableGenerators);
			return selectedGenerator.Generate(schema);
		}
	}
}
