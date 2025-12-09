using System;
using System.Net.Http;
using System.Text.Json;
using Json.More;
using Json.Schema.Tests;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.OpenApi.Tests;

public class SpecExampleTests
{
	[Test]
	public void ConformanceTest()
	{
		var onlineSchemaText = new HttpClient().GetStringAsync(MetaSchemas.DocumentSchema_31Id).Result;
		var onlineSchemaJson = JsonDocument.Parse(onlineSchemaText).RootElement;
		var onlineSchema = JsonSchema.Build(onlineSchemaJson, new BuildOptions{SchemaRegistry = new()});

		TestConsole.WriteLine(onlineSchema.Root.Source);
		TestConsole.WriteLine(MetaSchemas.DocumentSchema_31.Root.Source);

		Assert.That(() => onlineSchema.Root.Source.IsEquivalentTo(MetaSchemas.DocumentSchema_31.Root.Source));
	}

	[Test]
	public void DevTest()
	{
		var openApiDocText =
			"""
			{
			  "openapi": "3.1.0",
			  "info": {
			    "title": "Webhook Example",
			    "version": "1.0.0"
			  },
			  "webhooks": {
			    "newPet": {
			      "post": {
			        "requestBody": {
			          "description": "Information about a new pet in the system",
			          "content": {
			            "application/json": {
			              "schema": {
			                "$ref": "#/components/schemas/Pet"
			              }
			            }
			          }
			        },
			        "responses": {
			          "200": {
			            "description": "Return a 200 status to indicate that the data was received successfully"
			          }
			        }
			      }
			    }
			  },
			  "components": {
			    "schemas": {
			      "Pet": {
			        "required": [
			          "id",
			          "name"
			        ],
			        "properties": {
			          "id": {
			            "type": "integer",
			            "format": "int64"
			          },
			          "name": {
			            "type": "string"
			          },
			          "tag": {
			            "type": "string"
			          }
			        }
			      }
			    }
			  }
			}

			""";

		var openApiDocNode = JsonDocument.Parse(openApiDocText).RootElement;
		_ = new OpenApiDoc(new Uri("http://localhost:1234/openapi"), openApiDocNode);
		var refSchema = new JsonSchemaBuilder()
			.Schema(Schema.MetaSchemas.Draft201909Id)
			.Ref("http://localhost:1234/openapi#/webhooks/newPet/post/requestBody/content/application~1json/schema");

		var payload = JsonDocument.Parse(
			"""
			{
				"id": 1,
				"name": "fido",
				"tag": "a very good dog"
			}
			""").RootElement;

		var result = refSchema!.Evaluate(payload);

		result.AssertValid();
	}
}