using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class PatternTests
{
	[Test]
	public void DeserializingInvalidPatternDoesNotThrow()
	{
		var schemaText =
			"""
			   {
			  "pattern": "(.{2}"
			}
			""";

		var schema = JsonSerializer.Deserialize(schemaText, TestSerializerContext.Default.JsonSchema)!;

		JsonNode instance = "some string";

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}
	[Test]
	public void DeserializingInvalidPatternPropertyDoesNotThrow()
	{
		var schemaText =
			"""
			{
			  "patternProperties": {
				"(.{2}": true
			  }
			}
			""";

		var schema = JsonSerializer.Deserialize(schemaText, TestSerializerContext.Default.JsonSchema)!;

		var instance = JsonNode.Parse(
			"""
			{
			  "abc": 42
			}
			""");

		var result = schema.Evaluate(instance);

		result.AssertValid(); // property doesn't match because it's invalid
	}
}