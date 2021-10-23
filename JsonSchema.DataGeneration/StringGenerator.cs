using System;
using System.Text.Json;

namespace Json.Schema.DataGeneration;

public class StringGenerator : IDataGenerator
{
	public static StringGenerator Instance { get; } = new StringGenerator();

	private StringGenerator() { }

	public SchemaValueType Type => SchemaValueType.String;

	public JsonElement Generate(JsonSchema schema)
	{
		throw new NotImplementedException();
	}
}