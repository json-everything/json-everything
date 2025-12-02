using System.Text.Json.Nodes;

namespace Json.Schema.DataGeneration.Generators;

internal class NullGenerator : IDataGenerator
{
	public static NullGenerator Instance { get; } = new();

	private NullGenerator()
	{
	}

	public SchemaValueType Type => SchemaValueType.Null;

	public GenerationResult Generate(RequirementsContext context)
	{
		return GenerationResult.Success((JsonNode?)null);
	}
}