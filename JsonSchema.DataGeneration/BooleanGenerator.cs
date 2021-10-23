using System.Text.Json;
using Json.More;

namespace Json.Schema.DataGeneration;

public class BooleanGenerator : IDataGenerator
{
	public static BooleanGenerator Instance { get; } = new BooleanGenerator();

	private BooleanGenerator() { }

	public SchemaValueType Type => SchemaValueType.Boolean;

	public GenerationResult Generate(JsonSchema schema)
	{
		return GenerationResult.Success(JsonSchemaExtensions.Randomizer.Bool());
	}
}