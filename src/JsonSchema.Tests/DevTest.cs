using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		// TODO: add try-register at node level (may need to wrap in schema)
		var schemaText = """
		    {
		      "$id": "outer",
		      "$ref": "inner",
		      "$defs": {
		        "stringy-thingy": {
		          "$id": "inner",
		          "type": "string"
		        }
		      }
		    }
		    """;

		var schemaJson = JsonDocument.Parse(schemaText).RootElement;
		var schema = JsonSchema.Build(schemaJson);

		//var instanceText = "\"a string\"";
		var instanceText = "42";
		var instance = JsonDocument.Parse(instanceText).RootElement;

		var results = schema.Evaluate(instance);

		Console.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
	}
}