namespace Json.Schema.DataGeneration
{
	public interface IDataGenerator
	{
		SchemaValueType Type { get; }

		GenerationResult Generate(JsonSchema schema);
	}
}