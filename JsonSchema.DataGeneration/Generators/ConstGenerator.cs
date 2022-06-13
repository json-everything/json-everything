namespace Json.Schema.DataGeneration.Generators;

internal class ConstGenerator : IPrioritizedDataGenerator
{
	public static ConstGenerator Instance { get; } = new();

	private ConstGenerator() { }

	public bool Applies(RequirementsContext context)
	{
		return context.ConstIsSet;
	}

	public GenerationResult Generate(RequirementsContext context)
	{
		return GenerationResult.Success(context.Const);
	}
}