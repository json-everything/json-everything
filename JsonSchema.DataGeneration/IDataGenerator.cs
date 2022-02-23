namespace Json.Schema.DataGeneration
{
	internal interface IPrioritizedDataGenerator
	{
		bool Applies(RequirementsContext context);
		GenerationResult Generate(RequirementsContext context);
	}
	internal interface IDataGenerator
	{
		SchemaValueType Type { get; }

		GenerationResult Generate(RequirementsContext context);
	}
}