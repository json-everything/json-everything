using System;
using System.Text.Json;

namespace Json.Schema.DataGeneration;

public class ObjectGenerator : IDataGenerator
{
	public static ObjectGenerator Instance { get; } = new ObjectGenerator();

	private ObjectGenerator() { }

	public SchemaValueType Type => SchemaValueType.Object;

	public JsonElement Generate(JsonSchema schema)
	{
		throw new NotImplementedException();
	}
}