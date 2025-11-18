using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var schemaText = """
		    {
		      "type": "string"
		    }
		    """;

		var schemaJson = JsonDocument.Parse(schemaText).RootElement;
		var schema = JsonSchema.Build(schemaJson);

		var instanceText = "\"a string\"";
		var instance = JsonDocument.Parse(instanceText).RootElement;

		var results = schema.Evaluate(instance);
	}
}