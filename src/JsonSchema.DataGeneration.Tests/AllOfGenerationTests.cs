using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

internal class AllOfGenerationTests
{
	[Test]
	public void AllOfWithMinAndMaxNumber()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Minimum(10),
				new JsonSchemaBuilder().Maximum(20)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void AllOfWithDifferentTypesFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		var result = schema.GenerateData(buildOptions);

		Assert.That(result.IsSuccess, Is.False, "generation succeeded somehow");
	}
}