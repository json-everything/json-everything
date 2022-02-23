namespace Json.Schema.DataGeneration
{
	internal interface IDataGenerator
	{
		SchemaValueType Type { get; }

		GenerationResult Generate(RequirementsContext context);
	}
}