namespace Json.Schema.DataGeneration.Generators
{
	internal class EnumGenerator : IPrioritizedDataGenerator
	{
		public static EnumGenerator Instance { get; } = new EnumGenerator();

		private EnumGenerator() { }

		public bool Applies(RequirementsContext context)
		{
			return context.EnumOptions != null;
		}

		public GenerationResult Generate(RequirementsContext context)
		{
			var value = JsonSchemaExtensions.Randomizer.ListItem(context.EnumOptions);
			return GenerationResult.Success(value);
		}
	}
}