using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

internal class AllOfGenerationTests
{
	[Test]
	public void AllOfWithMinAndMaxNumber()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Minimum(10),
				new JsonSchemaBuilder().Maximum(20)
			);

		Run(schema);
	}

	[Test]
	public void AllOfWithDifferentTypesFails()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		var result = schema.GenerateData();

		Assert.That(result.IsSuccess, Is.False, "generation succeeded somehow");
	}
}