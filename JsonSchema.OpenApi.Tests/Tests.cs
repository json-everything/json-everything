using System;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.OpenApi.Tests;

public class SpecExampleTests
{
	[OneTimeSetUp]
	public void Setup()
	{
		Vocabularies.Register();

		ValidationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}

	[Test]
	public void ConformanceTest()
	{
		var onlineSchemaJson = new HttpClient().GetStringAsync(MetaSchemas.OpenApiDocumentSchemaId).Result;
		var onlineSchema = JsonSerializer.Deserialize<JsonSchema>(onlineSchemaJson);

		var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
		Console.WriteLine(JsonSerializer.Serialize(onlineSchema, options));
		Console.WriteLine(JsonSerializer.Serialize(MetaSchemas.DocumentSchema, options));
		Assert.AreEqual(onlineSchema, MetaSchemas.DocumentSchema);
	}
}