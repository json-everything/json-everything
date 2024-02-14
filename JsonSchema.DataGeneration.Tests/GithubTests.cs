using System;
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
			Console.WriteLine(JsonSerializer.Serialize(generationResult, TestHelpers.SerializerOptions));
		}
	}

	[Test]
	public void Issue646_EitherPropertyButNotBoth_WithNot()
	{
		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.OneOf(
				new JsonSchemaBuilder()
					.Properties(
						("A", new JsonSchemaBuilder()),
						("B", new JsonSchemaBuilder().Not(new JsonSchemaBuilder()))
					),
				new JsonSchemaBuilder()
					.Properties(
						("A", new JsonSchemaBuilder().Not(new JsonSchemaBuilder())),
						("B", new JsonSchemaBuilder())
					)
			)
			.UnevaluatedProperties(false)
			.Build();

		var generationResult = schema.GenerateData();
		Console.WriteLine(JsonSerializer.Serialize(generationResult.Result, TestHelpers.SerializerOptions));

		var result = schema.Evaluate(generationResult.Result);

		result.AssertValid();
	}

	[Test]
	public void Issue646_EitherPropertyButNotBoth_WithFalse()
	{
		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.OneOf(
				new JsonSchemaBuilder()
					.Properties(
						("A", new JsonSchemaBuilder()),
						("B", false)
					),
				new JsonSchemaBuilder()
					.Properties(
						("A", false),
						("B", new JsonSchemaBuilder())
					)
			)
			.UnevaluatedProperties(false)
			.Build();

		var generationResult = schema.GenerateData();
		Console.WriteLine(JsonSerializer.Serialize(generationResult.Result, TestHelpers.SerializerOptions));

		var result = schema.Evaluate(generationResult.Result);

		result.AssertValid();
	}

	[Test]
	public void Issue646_PropertyWithFalse()
	{
		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Type(SchemaValueType.Object)
			.Properties(
				("A", new JsonSchemaBuilder()),
				("B", false)
			)
			.MinProperties(4)
			.Build();

		var generationResult = schema.GenerateData();
		Console.WriteLine(JsonSerializer.Serialize(generationResult.Result, TestHelpers.SerializerOptions));

		var result = schema.Evaluate(generationResult.Result);

		result.AssertValid();
	}
}