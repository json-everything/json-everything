using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var schema1Text = """
		    {
		      "$id": "a",
		      "properties": {
		        "b": { "$ref": "b" },
		        "value": { "type": "boolean" }
		      }
		    }
		    """;

		var schema1Json = JsonDocument.Parse(schema1Text).RootElement;
		var schema1 = JsonSchema.Build(schema1Json);

		var schema2Text = """
		    {
		      "$id": "b",
		      "properties": {
		        "b": { "$ref": "a" },
		        "value": { "type": "string" }
		      }
		    }
		    """;

		var schema2Json = JsonDocument.Parse(schema2Text).RootElement;
		var schema2 = JsonSchema.Build(schema2Json);

		//var instanceText = "\"a string\"";
		var instanceText = """
			{
			  "value": "mineral",
			  "b": {
			    "value": true,
			    "a" : {
			      "value": "vegetable"
			    }
			  }
			}
			""";
		var instance = JsonDocument.Parse(instanceText).RootElement;

		var results = schema2.Evaluate(instance);

		Console.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
	}
}