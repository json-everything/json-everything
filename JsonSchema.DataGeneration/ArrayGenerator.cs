using System;
using System.Text.Json;

namespace Json.Schema.DataGeneration;

public class ArrayGenerator : IDataGenerator
{
	public static ArrayGenerator Instance { get; } = new ArrayGenerator();

	private ArrayGenerator() { }

	public SchemaValueType Type => SchemaValueType.Array;

	public JsonElement Generate(JsonSchema schema)
	{
		throw new NotImplementedException();
	}
}