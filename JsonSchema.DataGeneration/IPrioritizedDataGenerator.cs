namespace Json.Schema.DataGeneration
{
	internal interface IPrioritizedDataGenerator
	{
		bool Applies(RequirementsContext context);
		GenerationResult Generate(RequirementsContext context);
	}
}