using System.Text.Json;

namespace Json.Schema.DataGeneration;

public class NullGenerator : IDataGenerator
{
	private static readonly JsonElement _nullValue = JsonDocument.Parse("null").RootElement.Clone();

	public static NullGenerator Instance { get; } = new NullGenerator();

	private NullGenerator(){}

	public SchemaValueType Type => SchemaValueType.Null;

	public GenerationResult Generate(JsonSchema schema)
	{
		return GenerationResult.Success(_nullValue);
	}
}