using System.Text.Json;

namespace Json.Schema.DataGeneration;

public class NullGenerator : IDataGenerator
{
	private static readonly JsonElement _nullValue = JsonDocument.Parse("null").RootElement.Clone();

	public static NullGenerator Instance { get; } = new NullGenerator();

	private NullGenerator(){}

	public SchemaValueType Type => SchemaValueType.Null;

	public JsonElement Generate(JsonSchema schema)
	{
		return _nullValue.Clone();
	}
}