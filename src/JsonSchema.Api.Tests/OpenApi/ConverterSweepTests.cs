using System.Net;
using System.Text.Json;
using Json.Schema.Api.OpenApi;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Api.Tests.OpenApi;

public class ConverterSweepTests
{
	[Test]
	public void BuildAndSerialize()
	{
		var doc = new OpenApiDocument("3.1.0", new OpenApiInfo("Test API", "1.0")
		{
			Contact = new ContactInfo { Name = "me", Email = "me@you.com" },
			License = new LicenseInfo("MIT") { Identifier = "MIT" }
		})
		{
			Servers = [new Server("https://api.example.com") { Description = "prod" }],
			Paths = new PathCollection
			{
				["/pets/{petId}"] = new PathItem
				{
					Get = new Operation
					{
						OperationId = "getPet",
						Tags = ["pets"],
						Parameters = [new Parameter("petId", ParameterLocation.Path) { Required = true }],
						Responses = new ResponseCollection
						{
							[HttpStatusCode.OK] = new Response("ok")
							{
								Content = new() { ["application/json"] = new MediaType { Schema = Ref.To.Schema("Pet") } }
							},
							Default = new Response("unexpected")
						}
					}
				}
			},
			Components = new ComponentCollection
			{
				Schemas = new() { ["Pet"] = new JsonSchemaBuilder().Type(SchemaValueType.Object).Build() }
			}
		};

		var json = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
		TestConsole.WriteLine(json);

		Assert.That(json, Does.Contain("\"openapi\": \"3.1.0\""));
		Assert.That(json, Does.Contain("/pets/{petId}"));
		Assert.That(json, Does.Contain("\"operationId\": \"getPet\""));
		Assert.That(json, Does.Contain("#/components/schemas/Pet"));
		Assert.That(json, Does.Contain("\"in\": \"path\""));
		Assert.That(json, Does.Contain("\"200\""));
		Assert.That(json, Does.Contain("\"default\""));
	}

	[Test]
	public void RoundTripPreservesUnknownAndExtensionData()
	{
		var source = """
			{
			  "openapi": "3.1.0",
			  "info": { "title": "T", "version": "1", "x-vendor": 42, "futureField": "kept" },
			  "paths": {
			    "/a": {
			      "get": {
			        "operationId": "a",
			        "responses": { "200": { "description": "ok" } }
			      }
			    }
			  }
			}
			""";

		var doc = JsonSerializer.Deserialize<OpenApiDocument>(source)!;

		Assert.That(doc.Info.ExtensionData, Is.Not.Null);
		Assert.That(doc.Info.ExtensionData!["x-vendor"]!.GetValue<int>(), Is.EqualTo(42));
		Assert.That(doc.Info.UnknownData, Is.Not.Null);
		Assert.That(doc.Info.UnknownData!["futureField"]!.GetValue<string>(), Is.EqualTo("kept"));

		var json = JsonSerializer.Serialize(doc);
		TestConsole.WriteLine(json);

		Assert.That(json, Does.Contain("x-vendor"));
		Assert.That(json, Does.Contain("futureField"));
		Assert.That(json, Does.Contain("\"operationId\":\"a\""));
	}

	[Test]
	public void UnknownStatusCodeRoundTrips()
	{
		var source = """
			{
			  "openapi": "3.1.0",
			  "info": { "title": "T", "version": "1" },
			  "paths": {
			    "/a": {
			      "get": { "responses": { "2XX": { "description": "any success" } } }
			    }
			  }
			}
			""";

		var doc = JsonSerializer.Deserialize<OpenApiDocument>(source)!;
		var json = JsonSerializer.Serialize(doc);

		TestConsole.WriteLine(json);
		Assert.That(json, Does.Contain("2XX"));
	}
}
