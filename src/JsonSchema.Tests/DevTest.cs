using System;
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
		      "$ref": "#/$defs/stringy-thingy",
		      "$defs": {
		        "stringy-thingy": { "type": "string" }
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