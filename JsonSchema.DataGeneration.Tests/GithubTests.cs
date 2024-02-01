using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests;

internal class GithubTests
{
	[Test]
	public void Issue384_NodeAlreadyHasParent()
	{
		var schemaText = @"
            {
              ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
              ""title"": ""Product"",
              ""description"": ""A product from Acme's catalog"",
              ""type"": ""object"",
              ""properties"": {
                ""productId"": {
                  ""description"": ""The unique identifier for a product"",
                  ""type"": ""integer"",
                  ""default"": 3
                },
                ""productName"": {
                  ""description"": ""Name of the product"",
                  ""type"": ""string""
                },
                ""price"": {
                  ""description"": ""The price of the product"",
                  ""type"": ""number"",
                  ""exclusiveMinimum"": 0
                },
                ""propNotRequired"": {
                  ""description"": ""This is not required"",
                  ""type"": ""number""
                },
                ""enumPropertyHappy"": {
                    ""description"": ""Happy enum"",
                    ""type"": ""number"",
                    ""enum"": [3,5,1]
                }
              },
              ""required"": [ ""productId"", ""productName"", ""price"" ],
              ""additionalProperties"" : true
            }
            ";
		var schema = JsonSchema.FromText(schemaText);

		// This is intermittent, but 10 times seems to be enough to cause this to fail.
		for (int i = 0; i < 10; i++)
		{
			var generationResult = schema.GenerateData();
			var sampleData = generationResult.Result;

			Console.WriteLine("Generated data result: " + sampleData);
			Console.WriteLine(JsonSerializer.Serialize(generationResult, new JsonSerializerOptions(DataGenerationTestsSerializerContext.Default.Options)
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}));
		}
	}
}