namespace Json.Schema.DataGeneration.Generators
{
	internal class ConstGenerator : IPrioritizedDataGenerator
	{
		public static ConstGenerator Instance { get; } = new ConstGenerator();

		private ConstGenerator() { }

		public bool Applies(RequirementsContext context)
		{
			return context.Const.HasValue;
		}

		public GenerationResult Generate(RequirementsContext context)
		{
			return GenerationResult.Success(context.Const!.Value);
		}
	}
}