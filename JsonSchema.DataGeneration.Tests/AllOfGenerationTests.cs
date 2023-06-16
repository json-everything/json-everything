using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

internal class AllOfGenerationTests
{
	[Test]
	public async Task AllOfWithMinAndMaxNumber()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Minimum(10),
				new JsonSchemaBuilder().Maximum(20)
			);

		await Run(schema);
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

		Assert.IsFalse(result.IsSuccess, $"generation succeeded somehow");
	}
}