using System.Text.Json;

namespace Json.Schema.DataGeneration;

public interface IDataGenerator
{
	SchemaValueType Type { get; }

	JsonElement Generate(JsonSchema schema);
}