using System;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.OpenApi.Tests;

public class SpecExampleTests
{
	[OneTimeSetUp]
	public void Setup()
	{
		Vocabularies.Register();

		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}

	[Test]
	public void ConformanceTest()
	{
		var onlineSchemaJson = new HttpClient().GetStringAsync(MetaSchemas.OpenApiDocumentSchemaId).Result;
		var onlineSchema = JsonSerializer.Deserialize<JsonSchema>(onlineSchemaJson, TestEnvironment.SerializerOptions);

		var options = new JsonSerializerOptions(TestEnvironment.SerializerOptions) { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
		Console.WriteLine(JsonSerializer.Serialize(onlineSchema, options));
		Console.WriteLine(JsonSerializer.Serialize(MetaSchemas.DocumentSchema, options));

		var asNode = JsonSerializer.SerializeToNode(MetaSchemas.DocumentSchema, TestEnvironment.SerializerOptions);
		var onlineAsNode = JsonSerializer.SerializeToNode(onlineSchema, TestEnvironment.SerializerOptions);
		Assert.That(() => asNode.IsEquivalentTo(onlineAsNode));
	}

	[Test]
	public void DevTest()
	{
		var openApiDocText = @"{
  ""openapi"": ""3.1.0"",
  ""info"": {
    ""title"": ""Webhook Example"",
    ""version"": ""1.0.0""
  },
  ""webhooks"": {
    ""newPet"": {
      ""post"": {
        ""requestBody"": {
          ""description"": ""Information about a new pet in the system"",
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/Pet""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Return a 200 status to indicate that the data was received successfully""
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {
      ""Pet"": {
        ""required"": [
          ""id"",
          ""name""
        ],
        ""properties"": {
          ""id"": {
            ""type"": ""integer"",
            ""format"": ""int64""
          },
          ""name"": {
            ""type"": ""string""
          },
          ""tag"": {
            ""type"": ""string""
          }
        }
      }
    }
  }
}
";

		var openApiDocNode = JsonNode.Parse(openApiDocText);
		var openApiDoc = new OpenApiDoc(new Uri("http://localhost:1234/openapi"), openApiDocNode!);
		var targetSchema = openApiDoc.FindSubschema(JsonPointer.Parse("/webhooks/newPet/post/requestBody/content/application~1json/schema"), EvaluationOptions.Default);

		var payload = new JsonObject
		{
			["id"] = 1,
			["name"] = "fido",
			["tag"] = "a very good dog"
		};

		var result = targetSchema!.Evaluate(payload);

		result.AssertValid();
	}
}