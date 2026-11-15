using System.Text.Json;
using Json.Schema.Tests;
using NUnit.Framework;
using TestHelpers;

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
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText(schemaText, buildOptions);

		// This is intermittent, but 10 times seems to be enough to cause this to fail.
		for (int i = 0; i < 10; i++)
		{
			var generationResult = schema.GenerateData();
			var sampleData = generationResult.Result;

			TestConsole.WriteLine("Generated data result: " + sampleData);
			TestConsole.WriteLine(JsonSerializer.Serialize(generationResult, TestRunner.SerializerOptions));
		}
	}

	[Test]
	public void Issue646_EitherPropertyButNotBoth_WithNot()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
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
			.Build();

		var generationResult = schema.GenerateData();
		var resultElt = JsonSerializer.SerializeToElement(generationResult.Result, TestRunner.SerializerOptions);
		TestConsole.WriteLine(resultElt.GetRawText());

		var result = schema.Evaluate(resultElt);

		result.AssertValid();
	}

	[Test]
	public void Issue646_EitherPropertyButNotBoth_WithFalse()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
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
			.Build();

		var generationResult = schema.GenerateData();
		var resultElt = JsonSerializer.SerializeToElement(generationResult.Result, TestRunner.SerializerOptions);
		TestConsole.WriteLine(resultElt.GetRawText());

		var result = schema.Evaluate(resultElt);

		result.AssertValid();
	}

	[Test]
	public void Issue646_EitherPropertyButNotBoth_WithIntegerAndFalse()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft202012Id)
			.OneOf(
				new JsonSchemaBuilder()
					.Properties(
						("A", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
						("B", false)
					),
				new JsonSchemaBuilder()
					.Properties(
						("A", false),
						("B", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
					)
			)
			.Build();

		var generationResult = schema.GenerateData();
		var resultElt = JsonSerializer.SerializeToElement(generationResult.Result, TestRunner.SerializerOptions);
		TestConsole.WriteLine(resultElt.GetRawText());

		var result = schema.Evaluate(resultElt);

		result.AssertValid();
	}

	[Test]
	public void Issue646_PropertyWithFalse()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft202012Id)
			.Type(SchemaValueType.Object)
			.Properties(
				("A", new JsonSchemaBuilder()),
				("B", false)
			)
			.MinProperties(4)
			.Build();

		var generationResult = schema.GenerateData();
		var resultElt = JsonSerializer.SerializeToElement(generationResult.Result, TestRunner.SerializerOptions);
		TestConsole.WriteLine(resultElt.GetRawText());

		var result = schema.Evaluate(resultElt);

		result.AssertValid();
	}

	[Test]
	public void Issue647_EitherPropertyButNotBoth_WithUnevaluatedProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
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
		var resultElt = JsonSerializer.SerializeToElement(generationResult.Result, TestRunner.SerializerOptions);
		TestConsole.WriteLine(resultElt.GetRawText());

		var result = schema.Evaluate(resultElt);

		result.AssertValid();
	}
}