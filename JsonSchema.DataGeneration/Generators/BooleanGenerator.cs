namespace Json.Schema.DataGeneration.Generators
{
	internal class BooleanGenerator : IDataGenerator
	{
		public static BooleanGenerator Instance { get; } = new BooleanGenerator();

		private BooleanGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.Boolean;

		public GenerationResult Generate(RequirementsContext context)
		{
			return GenerationResult.Success(JsonSchemaExtensions.Randomizer.Bool());
		}
	}
}