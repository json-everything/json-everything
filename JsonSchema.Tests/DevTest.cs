using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("example-schema")
			.Type(SchemaValueType.Object)
			.Title("foo object schema")
			.Properties(
				("foo", new JsonSchemaBuilder()
					.Title("foo's title")
					.Description("foo's description")
					.Type(SchemaValueType.String)
				)
			)
			.Required("foo")
			.AdditionalProperties(false);

		var instance = new JsonObject { ["foo"] = new JsonObject { ["definitely"] = "not a string" } };

		var results = schema.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}));
	}
}