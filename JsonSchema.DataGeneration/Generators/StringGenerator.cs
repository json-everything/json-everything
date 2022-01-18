using System;

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
			return GenerationResult.Success("string");
		}
	}
}