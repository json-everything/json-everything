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
					.Pattern("^foo ")
					.MinLength(10)
				)
			)
			.Required("foo")
			.AdditionalProperties(false);

		var instance = new JsonObject { ["foo"] = "foo awe;ovinawe" };

		var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			Converters = { new Pre202012EvaluationResultsJsonConverter() }
		}));
	}
}